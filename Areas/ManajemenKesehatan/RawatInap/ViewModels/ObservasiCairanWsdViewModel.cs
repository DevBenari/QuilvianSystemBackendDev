namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.ViewModels
{
    public class ObservasiCairanWsdViewModel
    {
        public Guid KunjunganId { get; set; }
        public Guid PasienId { get; set; }
        public Guid UserActiveId { get; set; }

        public DateTime TglAwalObservasiWSD { get; set; }
        public DateTime TglAkhirObservasiWSD { get; set; }

        public decimal CairanSisaWSDSebelumnya { get; set; }
        public decimal CairanWSDBertambah { get; set; }
        public decimal CairanSisaWSDTabung { get; set; }

        public Guid TtdId { get; set; }
        public string PathTtd { get; set; }
        public string Keterangan { get; set; }
    }
}
