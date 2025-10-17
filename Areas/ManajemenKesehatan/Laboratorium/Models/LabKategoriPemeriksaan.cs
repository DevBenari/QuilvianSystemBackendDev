using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Models;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.Laboratorium.Models
{
    public class LabKategoriPemeriksaan : UserActivity
    {
        [Key]
        public Guid KategoriPemeriksaanId { get; set; }  // Generate otomatis
        public string? NamaKategori { get; set; }
        public string? KodeKategori { get; set; }
        public Guid? LabId { get; set; }
        public string? Keterangan { get; set; } // Keterangan tambahan
    }
}
