using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Kasir.ViewModels;

public class MainKasirViewModel
{
    [Required]
    public Guid? KunjunganId { get; set; }
    public Guid? DiskonId { get; set; }
    public Guid? PasienId { get; set; }
    public string? StatusPembayaran { get; set; }
    public bool? IsVerified { get; set; }
    public Guid? TTDUserVerfiedId { get; set; }
    public decimal? DepositRanap {  get; set; }
    public decimal? SisaDeposit {  get; set; }
    public decimal? JumlahPajak {  get; set; }
    public decimal? TotalBiayaObat { get; set; }
    public decimal? TotalBiayaTindakan { get; set; }
    public decimal? SubTotalMandiri { get; set; }
    public decimal? SubTotalAsuransi { get; set; }
    public decimal? SubTotalAsuransiExcess { get; set; }
    public decimal? HargaDiskon { get; set; }
    public decimal? TotalPembayaran { get; set; }
    public decimal? GrandTotalPembayaran { get; set; }
    public string? Keterangan { get; set; }
    public List<MainKasirDetailViewModel>? Details { get; set; }
}
