using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.RawatInap.Models
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowSpecific")]
    public class PengkajianKulit : UserActivity
    {
        [Key]
        public Guid IntegritasKulitId { get; set; }
        public Guid? KunjunganId { get; set; }
        public Guid? PengkajianPerawatId { get; set; }
        public bool? IsTerganggu { get; set; }
        public decimal? SkalaDekubitus { get; set; }
        [MaxLength(250)]
        public string? KondisiKulit { get; set; }
        public string? Keterangan { get; set; }
    }
}
