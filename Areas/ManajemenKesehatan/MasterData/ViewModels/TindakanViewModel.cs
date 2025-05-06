using QuilvianSystemBackendDev.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.ViewModels
{
    public class TindakanViewModel
    {
        public Guid TindakanId { get; set; }
        public string KodeTindakan { get; set; }
        public string NamaTindakan { get; set; }

    }

    public class TindakanAsuransiViewModel
    {
        public Guid TindakanAsuransiId { get; set; }
        public Guid TindakanId { get; set; }
        public Guid AsuransiId { get; set; }

    }

    public class TindakanPoliViewModel
    {
        public Guid TindakanPoliId { get; set; }
        public Guid TindakanId { get; set; }
        public Guid PoliId { get; set; }

    }
}
