using QuilvianSystem.Areas.Administration.Models;
using QuilvianSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystem.Areas.PatientRegistration.Models
{
    [Table("PtrReference", Schema = "dbo")]
    public class Reference : UserActivity
    {
        [Key]
        public Guid ReferenceId { get; set; }
        public string KodeRujukan { get; set; }
        public string NamaRujukan { get; set; } // Ex : Konsultasi, Luar RS, Atas Nama Sendiri

        public ICollection<ReferenceType> ReferenceType { get; set; }
    }

    [Table("PtrReferenceType", Schema = "dbo")]
    public class ReferenceType : UserActivity
    {
        [Key]
        public Guid ReferenceTypeId { get; set; }
        public string KodeTipeRujukan { get; set; }
        public string NamaTipeRujukan { get; set; } // Ex : RSU/RSK/RB, Puskesmas, Dukun Terlatih
        public Guid? ReferenceId { get; set; }  // Ex : Luar RS

        //Relationship
        [ForeignKey("ReferenceId")]
        public Reference? Reference { get; set; }
    }

    [Table("PtrReferenceDetail", Schema = "dbo")]
    public class ReferenceDetail : UserActivity
    {
        [Key]
        public Guid ReferenceDetailId { get; set; }
        public string KodeDetailRujukan { get; set; }
        public string NamaDetailRujukan { get; set; } // Ex : BALAI KESEHATA KEMENTERIAN KEUANGAN
        public string NomorTelepon { get; set; }
        public string Alamat { get; set; }
        public Guid? ReferenceTypeId { get; set; } // Ex : RSU/RSK/RB

        //Relationship
        [ForeignKey("ReferenceTypeId")]
        public ReferenceType? ReferenceType { get; set; }
    }
}
