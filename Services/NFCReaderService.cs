using PCSC;
using PCSC.Monitoring;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using PCSC.Exceptions;
using System.Text;

namespace QuilvianSystemBackendDev.Services
{
    public class NFCReaderService
    {
        private readonly ISCardContext _context;
        private readonly ISCardMonitor _monitor;
        private string? _currentReader;

        public NFCReaderService()
        {
            _context = ContextFactory.Instance.Establish(SCardScope.System);
            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
        }

        // Memulai monitoring pembaca NFC dan mendapatkan pembaca pertama
        public bool Start()
        {
            Console.WriteLine("🔄 Memulai monitoring NFC Reader...");

            var readerNames = _context.GetReaders();
            if (readerNames.Length == 0)
            {
                Console.WriteLine("❌ Tidak ada NFC Reader yang terdeteksi.");
                return false;
            }

            _currentReader = readerNames[0];
            Console.WriteLine($"✅ Memantau NFC di {_currentReader}...");

            _monitor.CardInserted += (sender, args) =>
            {
                Console.WriteLine($"🔹 Kartu NFC dimasukkan ke {args.ReaderName}");
            };

            _monitor.Start(_currentReader);
            return true;
        }

        // Mengambil pembaca yang aktif
        public string? GetCurrentReader()
        {
            return _currentReader;
        }

        public async Task<string> ReadNFCAsync()
        {
            try
            {
                if (_currentReader == null)
                {
                    return "❌ NFC Reader belum dimulai. Panggil `Start()` terlebih dahulu.";
                }

                return await ReadAllNFCDataAsync(_currentReader);
            }
            catch (PCSCException ex) when (ex.Message.Contains("The Smart card resource manager has shut down"))
            {
                Console.WriteLine("🔄 PCSC Service terdeteksi mati. Memulai ulang...");
                RestartPCSCService();
                return "❌ PCSC Service baru saja direstart. Silakan coba lagi.";
            }
            catch (Exception ex)
            {
                return $"❌ Error: {ex.Message}";
            }
        }
        public static string ToHexString(byte[] bytes)
        {
            return string.Join(" ", bytes.Select(b => b.ToString("X2")));
        }


        public async Task<string> ReadAllNFCDataAsync(string readerName)
        {
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = new SCardReader(context))
                {
                    if (reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any) != SCardError.Success)
                    {
                        return "❌ Gagal menghubungkan ke NFC Reader.";
                    }

                    var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);
                    StringBuilder allData = new StringBuilder();

                    // 🔥 1. Mendapatkan UUID kartu
                    byte[] getUIDCmd = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
                    byte[] uidResponse = new byte[256];
                    int uidReceived = uidResponse.Length;

                    if (reader.Transmit(sendPci, getUIDCmd, getUIDCmd.Length, new SCardPCI(), uidResponse, ref uidReceived) == SCardError.Success)
                    {
                        string uid = BitConverter.ToString(uidResponse, 0, uidReceived).Replace("-", " ");
                        allData.AppendLine($"🔑 **UUID Kartu:** {uid}\n");
                    }
                    else
                    {
                        allData.AppendLine("❌ Gagal membaca UUID kartu.\n");
                    }

                    // 🔥 2. Membaca Semua Sektor
                    for (int sector = 0; sector < 16; sector++)
                    {
                        allData.AppendLine($"\n🟢 **Sektor {sector}:**");

                        // Coba autentikasi dengan Key A (gunakan key default untuk MIFARE Classic)
                        byte[] keyA = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }; // Key default (coba gunakan yang sesuai)
                        byte[] authCmd = { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, (byte)(sector * 4), 0x60, 0x00 };
                        authCmd = authCmd.Concat(keyA).ToArray();

                        byte[] authResponse = new byte[256];
                        int authBytes = 0;

                        if (reader.Transmit(sendPci, authCmd, authCmd.Length, new SCardPCI(), authResponse, ref authBytes) != SCardError.Success)
                        {
                            allData.AppendLine($"❌ Gagal autentikasi untuk sektor {sector}.");
                            continue;
                        }

                        // Jika autentikasi berhasil, baca semua blok dalam sektor ini
                        for (int block = 0; block < 4; block++)
                        {
                            byte[] readCmd = { 0xFF, 0xB0, (byte)(sector * 4 + block), 0x00, 0x10 };
                            byte[] receiveData = new byte[16];
                            int bytesReceived = receiveData.Length;

                            if (reader.Transmit(sendPci, readCmd, readCmd.Length, new SCardPCI(), receiveData, ref bytesReceived) == SCardError.Success)
                            {
                                string hexData = BitConverter.ToString(receiveData, 0, bytesReceived).Replace("-", " ");
                                allData.AppendLine($"📡 Blok {block}: {hexData}");

                                // Coba konversi ke UTF-8 jika mungkin
                                string utf8Data = Encoding.UTF8.GetString(receiveData).Replace("\0", "").Trim();
                                if (!string.IsNullOrEmpty(utf8Data))
                                {
                                    allData.AppendLine($"🔎 **Decoded Data (UTF-8):** {utf8Data}");
                                }
                            }
                            else
                            {
                                allData.AppendLine($"❌ Gagal membaca Blok {block} di sektor {sector}.");
                            }
                        }
                    }

                    return allData.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"❌ Error: {ex.Message}";
            }
        }
        public async Task<string> WriteHelloWorldToAnySector(string readerName)
        {
            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = new SCardReader(context))
                {
                    // 🔹 Hubungkan ke NFC Reader
                    if (reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any) != SCardError.Success)
                    {
                        return "❌ Gagal menghubungkan ke NFC Reader.";
                    }

                    var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);
                    string dataToWrite = "Hello World";
                    byte[] dataBytes = Encoding.UTF8.GetBytes(dataToWrite.PadRight(16, '\0')); // Maksimum 16 byte per blok

                    byte[][] possibleKeys = new byte[][]
                    {
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, // Key Default
                new byte[] { 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 }, // Key Umum 1
                new byte[] { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 }, // Key Umum 2
                new byte[] { 0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5 }, // Key Umum 3
                new byte[] { 0x4D, 0x3A, 0x99, 0xC3, 0x51, 0xDD }, // Key Umum 4
                    };

                    // 🔥 Coba menulis ke sektor 1 hingga 15
                    for (int sector = 14; sector < 16; sector++)
                    {
                        for (int blockOffset = 0; blockOffset < 3; blockOffset++) // Coba Blok 4, 5, 6
                        {
                            int blockToWrite = sector * 4 + blockOffset;

                            foreach (var key in possibleKeys)
                            {
                                // 🔹 Coba autentikasi dengan Key A
                                byte[] authCmd = { 0xFF, 0x86, 0x00, 0x00, 0x05, 0x01, (byte)blockToWrite, 0x60, 0x00 };
                                authCmd = authCmd.Concat(key).ToArray();

                                byte[] authResponse = new byte[256];
                                int authBytes = 0;
                                var authResult = reader.Transmit(sendPci, authCmd, authCmd.Length, new SCardPCI(), authResponse, ref authBytes);

                                if (authResult == SCardError.Success)
                                {
                                    // 🔹 Perintah Write untuk Blok
                                    byte[] writeCmd = { 0xFF, 0xD6, 0x00, (byte)blockToWrite, 0x10 };
                                    writeCmd = writeCmd.Concat(dataBytes).ToArray();

                                    byte[] writeResponse = new byte[2];
                                    int writeResponseLength = writeResponse.Length;
                                    var writeResult = reader.Transmit(sendPci, writeCmd, writeCmd.Length, new SCardPCI(), writeResponse, ref writeResponseLength);

                                    if (writeResult == SCardError.Success)
                                    {
                                        // 🔍 Verifikasi hasil penulisan
                                        byte[] readCmd = { 0xFF, 0xB0, (byte)blockToWrite, 0x00, 0x10 };
                                        byte[] receiveData = new byte[16];
                                        int bytesReceived = receiveData.Length;

                                        string verificationMessage = "";
                                        if (reader.Transmit(sendPci, readCmd, readCmd.Length, new SCardPCI(), receiveData, ref bytesReceived) == SCardError.Success)
                                        {
                                            string hexData = BitConverter.ToString(receiveData, 0, bytesReceived).Replace("-", " ");
                                            string utf8Data = Encoding.UTF8.GetString(receiveData).Replace("\0", "").Trim();
                                            verificationMessage = $"📡 Blok {blockToWrite}: {hexData} ({utf8Data})";
                                        }

                                        return $"✅ Data berhasil ditulis ke sektor {sector}, blok {blockToWrite}!\n🔍 **Verifikasi Data yang Ditulis:**\n{verificationMessage}";
                                    }
                                }
                            }
                        }
                    }

                    return "❌ Tidak ada sektor yang bisa ditulis.";
                }
            }
            catch (Exception ex)
            {
                return $"❌ Error: {ex.Message}";
            }
        }


        private void RestartPCSCService()
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Process.Start(new ProcessStartInfo("cmd.exe", "/c net stop SCardSvr && net start SCardSvr")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo("bash", "-c 'sudo systemctl restart pcscd'")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }

                Console.WriteLine("✅ PCSC Service berhasil dimulai ulang.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Gagal memulai ulang PCSC Service: {ex.Message}");
            }
        }
    }
}


//using PCSC;
//using PCSC.Monitoring;
//using System;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using PCSC.Exceptions;

//namespace QuilvianSystemBackendDev.Services
//{
//    public class NFCReaderService
//    {
//        private readonly ISCardContext _context;
//        private readonly ISCardMonitor _monitor;
//        private string? _currentReader;

//        public NFCReaderService()
//        {
//            _context = ContextFactory.Instance.Establish(SCardScope.System);
//            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
//        }

//        // Memulai monitoring pembaca NFC dan mendapatkan pembaca pertama
//        public bool Start()
//        {
//            Console.WriteLine("🔄 Memulai monitoring NFC Reader...");

//            var readerNames = _context.GetReaders();
//            if (readerNames.Length == 0)
//            {
//                Console.WriteLine("❌ Tidak ada NFC Reader yang terdeteksi.");
//                return false;
//            }

//            _currentReader = readerNames[0];
//            Console.WriteLine($"✅ Memantau NFC di {_currentReader}...");

//            _monitor.CardInserted += (sender, args) =>
//            {
//                Console.WriteLine($"🔹 Kartu NFC dimasukkan ke {args.ReaderName}");
//            };

//            _monitor.Start(_currentReader);
//            return true;
//        }

//        // Mengambil pembaca yang aktif
//        public string? GetCurrentReader()
//        {
//            return _currentReader;
//        }

//        public async Task<string> ReadNFCAsync()
//        {
//            try
//            {
//                if (_currentReader == null)
//                {
//                    return "❌ NFC Reader belum dimulai. Panggil `Start()` terlebih dahulu.";
//                }

//                return await ReadMifareClassicDataAsync(_currentReader);
//            }
//            catch (PCSCException ex) when (ex.Message.Contains("The Smart card resource manager has shut down"))
//            {
//                Console.WriteLine("🔄 PCSC Service terdeteksi mati. Memulai ulang...");
//                RestartPCSCService();
//                return "❌ PCSC Service baru saja direstart. Silakan coba lagi.";
//            }
//            catch (Exception ex)
//            {
//                return $"❌ Error: {ex.Message}";
//            }
//        }

//        public async Task<string> ReadMifareClassicDataAsync(string readerName)
//        {
//            try
//            {
//                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
//                using (var reader = new SCardReader(context))
//                {
//                    if (reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any) != SCardError.Success)
//                    {
//                        return "❌ Gagal menghubungkan ke NFC Reader.";
//                    }

//                    var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);

//                    // Autentikasi dengan Key A untuk sektor yang relevan (misalnya sektor 1)
//                    byte[] authenticateCmd = new byte[]
//                    {
//                0xFF, 0x88, 0x00, 0x01, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
//                    }; // Ini adalah perintah autentikasi menggunakan Key A pada sektor 1

//                    int bytesReceived = 0;
//                    byte[] receiveBuffer = new byte[256];

//                    // Mengirim perintah autentikasi
//                    if (reader.Transmit(sendPci, authenticateCmd, authenticateCmd.Length, new SCardPCI(), receiveBuffer, ref bytesReceived) == SCardError.Success)
//                    {
//                        Console.WriteLine("✅ Autentikasi berhasil.");

//                        // Jika autentikasi berhasil, kita bisa membaca blok dari sektor yang relevan
//                        byte[] readMemoryCmd = { 0xFF, 0xB0, 0x00, 0x01, 0x10 }; // Baca blok pertama sektor 1
//                        byte[] receiveDataBuffer = new byte[256];
//                        int bytesReceivedRead = receiveDataBuffer.Length;

//                        if (reader.Transmit(sendPci, readMemoryCmd, readMemoryCmd.Length, new SCardPCI(), receiveDataBuffer, ref bytesReceivedRead) == SCardError.Success)
//                        {
//                            // Dekode hasilnya menjadi string
//                            string decodedData = System.Text.Encoding.UTF8.GetString(receiveDataBuffer, 0, bytesReceivedRead).TrimEnd('\0');
//                            return $"✅ Data NFC yang dibaca: {decodedData}";
//                        }
//                        else
//                        {
//                            return "❌ Gagal membaca blok memori.";
//                        }
//                    }
//                    else
//                    {
//                        return "❌ Gagal melakukan autentikasi.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return $"❌ Error: {ex.Message}";
//            }
//        }



//        public async Task<string> WriteNFCDataAsync(string readerName, string dataToWrite)
//        {
//            try
//            {
//                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
//                using (var reader = new SCardReader(context))
//                {
//                    // Cek koneksi dengan pembaca NFC
//                    if (reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any) != SCardError.Success)
//                    {
//                        return "❌ Gagal menghubungkan ke NFC Reader.";
//                    }

//                    byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataToWrite);

//                    int blockSize = 16;
//                    byte[] dataToSend = new byte[blockSize];
//                    Array.Copy(dataBytes, dataToSend, Math.Min(dataBytes.Length, blockSize));

//                    var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);

//                    byte[] writeMemoryCmd = new byte[] { 0xFF, 0xD6, 0x00, 0x00, (byte)dataToSend.Length };
//                    byte[] writeDataCmd = new byte[writeMemoryCmd.Length + dataToSend.Length];
//                    writeMemoryCmd.CopyTo(writeDataCmd, 0);
//                    dataToSend.CopyTo(writeDataCmd, writeMemoryCmd.Length);

//                    int bytesReceived = 0;
//                    if (reader.Transmit(sendPci, writeDataCmd, writeDataCmd.Length, new SCardPCI(), null, ref bytesReceived) == SCardError.Success)
//                    {
//                        return "✅ Data berhasil ditulis ke NFC!";
//                    }
//                    else
//                    {
//                        return "❌ Gagal menulis data ke NFC.";
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                return $"❌ Error: {ex.Message}";
//            }
//        }

//        private void RestartPCSCService()
//        {
//            try
//            {
//                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
//                {
//                    Process.Start(new ProcessStartInfo("cmd.exe", "/c net stop SCardSvr && net start SCardSvr")
//                    {
//                        RedirectStandardOutput = true,
//                        UseShellExecute = false,
//                        CreateNoWindow = true
//                    });
//                }
//                else
//                {
//                    Process.Start(new ProcessStartInfo("bash", "-c 'sudo systemctl restart pcscd'")
//                    {
//                        RedirectStandardOutput = true,
//                        UseShellExecute = false,
//                        CreateNoWindow = true
//                    });
//                }

//                Console.WriteLine("✅ PCSC Service berhasil dimulai ulang.");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Gagal memulai ulang PCSC Service: {ex.Message}");
//            }
//        }
//    }
//}


////using PCSC;
////using PCSC.Monitoring;
////using System;
////using System.Threading.Tasks;

////namespace QuilvianSystemBackendDev.Services
////{
////    public class NFCReaderService
////    {
////        private readonly ISCardContext _context;
////        private readonly ISCardMonitor _monitor;
////        private TaskCompletionSource<string>? _tcs;

////        public NFCReaderService()
////        {
////            _context = ContextFactory.Instance.Establish(SCardScope.System);
////            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
////        }

////        public void Start()
////        {
////            var readerNames = _context.GetReaders();
////            if (readerNames.Length == 0)
////            {
////                Console.WriteLine("❌ Tidak ada pembaca NFC yang terdeteksi.");
////                return;
////            }

////            var reader = readerNames[0];
////            Console.WriteLine($"✅ Memantau NFC di {reader}...");

////            _monitor.CardInserted += (sender, args) =>
////            {
////                Console.WriteLine($"🔹 Kartu NFC dimasukkan ke {args.ReaderName}");
////                try
////                {
////                    string uid = ReadCardUID(args.ReaderName);
////                    _tcs.TrySetResult(uid);
////                }
////                catch (Exception ex)
////                {
////                    _tcs.TrySetResult($"❌ Error membaca kartu: {ex.Message}");
////                }
////            };

////            _monitor.Start(reader);
////        }

////        public Task<string> ReadNFCAsync()
////        {
////            var readerNames = _context.GetReaders();
////            if (readerNames.Length == 0)
////            {
////                return Task.FromResult("❌ Tidak ada pembaca NFC yang terdeteksi.");
////            }

////            _tcs = new TaskCompletionSource<string>();

////            return _tcs.Task;
////        }

////        private string ReadCardUID(string readerName)
////        {
////            using (var context = ContextFactory.Instance.Establish(SCardScope.System))
////            using (var reader = new SCardReader(context))
////            {
////                var response = reader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
////                if (response != SCardError.Success)
////                {
////                    return $"❌ Gagal menghubungkan ke pembaca NFC: {response}";
////                }

////                var sendPci = SCardPCI.GetPci(reader.ActiveProtocol);
////                byte[] getUIDCmd = { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
////                byte[] receiveBuffer = new byte[256];
////                int bytesReceived = receiveBuffer.Length;

////                response = reader.Transmit(sendPci, getUIDCmd, getUIDCmd.Length, new SCardPCI(), receiveBuffer, ref bytesReceived);
////                if (response == SCardError.Success)
////                {
////                    return BitConverter.ToString(receiveBuffer, 0, bytesReceived).Replace("-", "");
////                }
////                return $"❌ Gagal membaca UID: {response}";
////            }
////        }
////    }
////}
