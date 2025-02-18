using PCSC.Monitoring;
using PCSC;

namespace QuilvianSystemBackendDev.Services
{
    public class gfjhNFCReaderService
    {
        private readonly ISCardContext _context;
        private readonly ISCardMonitor _monitor;

        public gfjhNFCReaderService()
        {
            _context = ContextFactory.Instance.Establish(SCardScope.System);
            _monitor = MonitorFactory.Instance.Create(SCardScope.System);
        }

        public void Start()
        {
            var readerNames = _context.GetReaders();
            if (readerNames.Length == 0)
            {
                Console.WriteLine("Tidak ada pembaca NFC yang terdeteksi.");
                return;
            }

            var reader = readerNames[0];

            _monitor.CardInserted += (sender, args) =>
            {
                Console.WriteLine($"Kartu NFC dimasukkan ke {args.ReaderName}");
                // Tambahkan logika membaca UID kartu
            };

            _monitor.Start(reader);
            Console.WriteLine($"Memantau NFC di {reader}");
        }
    }
}
