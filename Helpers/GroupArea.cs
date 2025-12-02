using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace QuilvianSystemBackendDev.Helpers
{
    public class GroupArea : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var ns = controller.ControllerType.Namespace ?? "";


            if (ns.Contains(".Areas.ManajemenKesehatan.MasterData"))
            {
                controller.ApiExplorer.GroupName = "master";
            }
            else if (ns.Contains(".Areas.ManajemenKesehatan"))
            {
                controller.ApiExplorer.GroupName = "manajemen_kesehatan";
            }
            else if (ns.Contains(".Areas.Administrator"))
            {
                controller.ApiExplorer.GroupName = "administrator";
            }
            else if (ns.Contains(".Areas.HRD"))
            {
                controller.ApiExplorer.GroupName = "hrd";
            }
            else
            {
                controller.ApiExplorer.GroupName = "v1";
            }
        }
    }
}
