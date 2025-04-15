using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models
{
    [Table("MstSkalaPain", Schema = "public")]
    public class SkalaPain : UserActivity
    {
        [Key]   
        public Guid SkalaPainId { get; set; }
        public Guid? KunjunganId { get; set; }
        public string KodeSkalaPain { get; set; }
        public string? ScoreSkalaPain { get; set; }
        public string? Deskripsi { get; set; }
        public string? KategoriSkala { get; set; }
        [NotMapped]
        public KategoriSkalaPainEnum? KategoriSkalaEnum
        {
            get => Enum.TryParse<KategoriSkalaPainEnum>(KategoriSkala, out var result) ? result : null;
            set => KategoriSkala = value?.ToString();
        }
    }
}
