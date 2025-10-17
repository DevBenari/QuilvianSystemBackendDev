using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;


namespace QuilvianSystemBackendDev.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CdssController : ControllerBase
    {
        // HIPERTENSI
        [HttpPost("hipertensi/diagnosa")]
        public IActionResult DiagnosaHipertensi([FromBody] TekananDarahModel pasien)
        {
            var hasil = new List<string>();
            var gejala = new List<string>
            {
                "Sakit kepala",
                "Pusing",
                "Penglihatan kabur",
                "Mual",
                "Sesak napas"
            };
            var rekomendasi = new List<string>();

            if (pasien.Sistolik >= 140 || pasien.Diastolik >= 90)
                hasil.Add("Hipertensi (klinik): Tekanan darah ≥ 140/90 mmHg");

            if (pasien.SistolikHome >= 135 || pasien.DiastolikHome >= 85)
                hasil.Add("Hipertensi (home): Tekanan darah ≥ 135/85 mmHg");

            if (pasien.SudahMinumObat)
                hasil.Add("Pasien sudah menggunakan obat antihipertensi");

            if (hasil.Count == 0)
                hasil.Add("Tidak terindikasi hipertensi berdasarkan data saat ini.");

            rekomendasi.AddRange(new string[]
            {
                "Kontrol tekanan darah secara rutin.",
                "Hidup sehat dengan diet rendah garam dan olahraga teratur.",
                "Jika sudah minum obat, patuhi anjuran dokter.",
                "Konsultasi ke dokter jika gejala memburuk atau tekanan darah tidak terkontrol."
            });

            var referensi = new List<object>
            {
                new {
                    Sumber = "PNPK 2021 – Tata Laksana Hipertensi Dewasa (Kemenkes RI)",
                    Link = "https://www.kemkes.go.id/id/pnpk-2021---tata-laksana-hipertensi-dewasa",
                    Kategori = "Diagnosa & Terapi"
                },
                new {
                    Sumber = "WHO – Pharmacological treatment of hypertension in adults (Full Guideline)",
                    Link = "https://www.who.int/publications/i/item/9789240033986",
                    Kategori = "Diagnosa & Terapi"
                },
                new {
                    Sumber = "American Heart Association – High Blood Pressure Resources",
                    Link = "https://www.heart.org/en/health-topics/high-blood-pressure",
                    Kategori = "Gejala & Edukasi"
                }
            };

            return Ok(new
            {
                Diagnosa = hasil,
                GejalaUmum = gejala,
                Rekomendasi = rekomendasi,
                Referensi = referensi,
                FhirInfo = new
                {
                    Version = "FHIR R4",
                    Resources = new
                    {
                        Observation = new
                        {
                            PanelBloodPressure = new
                            {
                                coding = new[] {
                                new { system = "http://loinc.org", code = "85354-9", display = "Blood pressure panel with all children optional" }
                            },
                                        text = "Blood pressure"
                                    },
                                    Systolic = new
                                    {
                                        coding = new[] {
                                new { system = "http://loinc.org", code = "8480-6", display = "Systolic blood pressure" }
                            },
                                        unit = new { system = "http://unitsofmeasure.org", code = "mm[Hg]", display = "mmHg" }
                                    },
                                    Diastolic = new
                                    {
                                        coding = new[] {
                                new { system = "http://loinc.org", code = "8462-4", display = "Diastolic blood pressure" }
                            },
                                        unit = new { system = "http://unitsofmeasure.org", code = "mm[Hg]", display = "mmHg" }
                                    }
                                },
                                Condition = new
                                {
                                    Hypertension = new
                                    {
                                        coding = new[] {
                                new { system = "http://snomed.info/sct", code = "38341003", display = "Hypertensive disorder, systemic arterial (disorder)" }
                            },
                                text = "Hypertension"
                            }
                        },
                        CarePlan = new
                        {
                            Recommendations = new[] {
                                new { text = "Kontrol tekanan darah secara rutin." },
                                new { text = "Diet rendah garam dan olahraga teratur." },
                                new { text = "Patuhi anjuran dokter jika sudah minum obat." }
                            }
                        }
                    }
                }
            });
        }

        // ANEMIA
        [HttpPost("anemia/diagnosa")]
        public IActionResult DiagnosaAnemia([FromBody] AnemiaModel pasien)
        {
            double ambangHb;

            if (pasien.UmurBulan >= 6 && pasien.UmurBulan <= 59)
                ambangHb = 11.0;
            else if (pasien.UmurTahun >= 5 && pasien.UmurTahun <= 11)
                ambangHb = 11.5;
            else if (pasien.UmurTahun >= 12 && pasien.UmurTahun <= 14)
                ambangHb = 12.0;
            else if (pasien.UmurTahun >= 15)
            {
                if (pasien.JenisKelamin.ToLower() == "pria")
                    ambangHb = 13.0;
                else if (pasien.SedangHamil)
                    ambangHb = 11.0;
                else
                    ambangHb = 12.0;
            }
            else
            {
                return BadRequest("Umur tidak valid untuk diagnosis anemia berdasarkan WHO.");
            }

            var hasil = new List<string>();
            if (pasien.Hb < ambangHb)
            {
                hasil.Add($"Terindikasi anemia: Hb {pasien.Hb} g/dL < {ambangHb} g/dL");
                if (pasien.Hb < 7)
                    hasil.Add("Anemia berat, segera konsultasi ke dokter.");
            }
            else
            {
                hasil.Add($"Tidak terindikasi anemia (Hb cukup: {pasien.Hb} g/dL)");
            }

            var gejala = new List<string>();
            if (pasien.Lemas) gejala.Add("Lemas/lelah");
            if (pasien.Pucat) gejala.Add("Pucat");
            if (pasien.SesakNapas) gejala.Add("Sesak napas");
            if (pasien.Pusing) gejala.Add("Pusing");
            if (pasien.Palpitasi) gejala.Add("Palpitasi (jantung berdebar)");

            var rekomendasi = new List<string>();
            if (pasien.Hb < ambangHb)
            {
                rekomendasi.Add("Konsultasi ke dokter untuk pemeriksaan lebih lanjut.");
                rekomendasi.Add("Konsumsi makanan kaya zat besi dan vitamin C.");
                rekomendasi.Add("Jika anemia berat, mungkin diperlukan terapi medis.");
            }
            else
            {
                rekomendasi.Add("Pertahankan pola makan sehat dan rutin cek kesehatan.");
            }

            var referensi = new List<object>
            {
                new {
                    Sumber = "WHO – Anaemia guidelines",
                    Link = "https://www.who.int/health-topics/anaemia#tab=tab_1",
                    Kategori = "Diagnosa & Terapi"
                },
                new {
                    Sumber = "Kemenkes RI – Pedoman Anemia",
                    Link = "https://www.kemkes.go.id/resources/download/pusdatin/infodatin/infodatin_anemia.pdf",
                    Kategori = "Diagnosa & Terapi"
                }
            };

            return Ok(new
            {
                Diagnosa = hasil,
                Gejala = gejala,
                Rekomendasi = rekomendasi,
                Referensi = referensi
            });
        }

        // DBD
        [HttpPost("dbd/diagnosa")]
        public IActionResult DiagnosaDbd([FromBody] DbdModel pasien)
        {
            var hasil = new List<string>();
            var rekomendasi = new List<string>();

            int gejalaPositif = 0;
            if (pasien.Demam >= 38) gejalaPositif++;
            if (pasien.NyeriOtot) gejalaPositif++;
            if (pasien.NyeriSendi) gejalaPositif++;
            if (pasien.NyeriBelakangMata) gejalaPositif++;
            if (pasien.Ruah) gejalaPositif++;

            bool trombositRendah = pasien.Trombosit < 150000;
            bool hematokritTinggi = pasien.Hematokrit > 45;

            if (gejalaPositif >= 3 && trombositRendah)
                hasil.Add("Terindikasi Demam Berdarah Dengue (DBD) berdasarkan gejala dan trombosit rendah.");
            else if (gejalaPositif >= 2)
                hasil.Add("Kemungkinan DBD, perlu pemeriksaan laboratorium lebih lanjut.");
            else
                hasil.Add("Tidak terindikasi DBD berdasarkan data yang diberikan.");

            if (trombositRendah)
            {
                rekomendasi.Add("Pantau trombosit secara berkala.");
                rekomendasi.Add("Jika trombosit < 50.000, segera rujuk ke rumah sakit.");
            }
            if (hematokritTinggi)
            {
                rekomendasi.Add("Waspadai tanda kebocoran plasma, segera evaluasi medis.");
            }
            if (!hasil.Contains("Tidak terindikasi DBD berdasarkan data yang diberikan."))
            {
                rekomendasi.Add("Istirahat yang cukup dan konsumsi cairan yang adekuat.");
                rekomendasi.Add("Pantau tanda-tanda vital dan segera konsultasi dokter.");
            }

            var referensi = new List<object>
            {
                new {
                    Sumber = "WHO – Dengue Guidelines for Diagnosis, Treatment, Prevention and Control",
                    Link = "https://www.who.int/publications/i/item/9789241547871",
                    Kategori = "Diagnosa & Terapi"
                },
                new {
                    Sumber = "Kemenkes RI – Pedoman Penatalaksanaan Demam Berdarah Dengue",
                    Link = "https://covid19.kemkes.go.id/pedoman-dbd",
                    Kategori = "Diagnosa & Terapi"
                },
                new {
                    Sumber = "CDC – Dengue Clinical Information",
                    Link = "https://www.cdc.gov/dengue/clinicallab/clinical.html",
                    Kategori = "Gejala & Diagnosa"
                }
            };

            return Ok(new
            {
                Diagnosa = hasil,
                Rekomendasi = rekomendasi,
                Referensi = referensi
            });
        }
    }

    // MODEL HIPERTENSI
    public class TekananDarahModel
    {
        public int Sistolik { get; set; }
        public int Diastolik { get; set; }
        public int SistolikHome { get; set; }
        public int DiastolikHome { get; set; }
        public bool SudahMinumObat { get; set; }
    }

    // MODEL ANEMIA
    public class AnemiaModel
    {
        public int UmurTahun { get; set; }
        public int UmurBulan { get; set; }
        public string JenisKelamin { get; set; }
        public bool SedangHamil { get; set; }
        public double Hb { get; set; }
        public bool Lemas { get; set; }
        public bool Pucat { get; set; }
        public bool SesakNapas { get; set; }
        public bool Pusing { get; set; }
        public bool Palpitasi { get; set; }
    }

    // MODEL DBD
    public class DbdModel
    {
        public double Demam { get; set; }
        public bool NyeriOtot { get; set; }
        public bool NyeriSendi { get; set; }
        public bool NyeriBelakangMata { get; set; }
        public bool Ruah { get; set; }
        public int Trombosit { get; set; }
        public double Hematokrit { get; set; }
    }

    //Hipertensi Ringan
    //    {
    //  "sistolik": 120,
    //  "diastolik": 75,
    //  "sistolikHome": 115,
    //  "diastolikHome": 70,
    //  "sudahMinumObat": false
    //}

    //    Hipertensi Sedang
    //    {
    //  "sistolik": 145,
    //  "diastolik": 95,
    //  "sistolikHome": 140,
    //  "diastolikHome": 90,
    //  "sudahMinumObat": true
    //}

    //    Hipertensi Berat
    //    {
    //  "sistolik": 180,
    //  "diastolik": 110,
    //  "sistolikHome": 175,
    //  "diastolikHome": 105,
    //  "sudahMinumObat": true
    //}

    //---

    //Anemia Ringan
    //    {
    //  "umurTahun": 25,
    //  "umurBulan": 0,
    //  "jenisKelamin": "wanita",
    //  "sedangHamil": false,
    //  "hb": 11.5,
    //  "lemas": false,
    //  "pucat": false,
    //  "sesakNapas": false,
    //  "pusing": false,
    //  "palpitasi": false
    //}

    //    Anemia Sedang
    //    {
    //  "umurTahun": 30,
    //  "umurBulan": 0,
    //  "jenisKelamin": "pria",
    //  "sedangHamil": false,
    //  "hb": 9.0,
    //  "lemas": true,
    //  "pucat": true,
    //  "sesakNapas": false,
    //  "pusing": true,
    //  "palpitasi": false
    //}

    //    Anemia Berat
    //    {
    //  "umurTahun": 40,
    //  "umurBulan": 0,
    //  "jenisKelamin": "wanita",
    //  "sedangHamil": true,
    //  "hb": 7.0,
    //  "lemas": true,
    //  "pucat": true,
    //  "sesakNapas": true,
    //  "pusing": true,
    //  "palpitasi": true
    //}

    //---

    //DBD Ringan
    //    {
    //  "demam": 38,
    //  "nyeriOtot": false,
    //  "nyeriSendi": false,
    //  "nyeriBelakangMata": false,
    //  "ruah": false,
    //  "trombosit": 150000,
    //  "hematokrit": 40.0
    //}

    //    DBD Sedang
    //    {
    //  "demam": 39,
    //  "nyeriOtot": true,
    //  "nyeriSendi": true,
    //  "nyeriBelakangMata": true,
    //  "ruah": true,
    //  "trombosit": 90000,
    //  "hematokrit": 45.0
    //}

    //    DBD Berat
    //    {
    //  "demam": 40,
    //  "nyeriOtot": true,
    //  "nyeriSendi": true,
    //  "nyeriBelakangMata": true,
    //  "ruah": true,
    //  "trombosit": 35000,
    //  "hematokrit": 50.0
    //}

}
