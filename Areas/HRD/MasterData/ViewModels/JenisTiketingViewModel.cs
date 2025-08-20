namespace QuilvianSystemBackendDev.Areas.HRD.MasterData.ViewModels
{
    public class JenisTiketingViewModel
    {
        public Guid JenisTicketId { get; set; }

        public Guid DepartementId { get; set; }

        public string NamaTicket { get; set; } = string.Empty;

        public string? Keterangan { get; set; }
    }
}
