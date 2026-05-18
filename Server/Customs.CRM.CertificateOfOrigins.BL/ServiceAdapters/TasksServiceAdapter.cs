using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Customs.Infrastructure.Tasks.ExternalCommon;
using Customs.Infrastructure.Tasks.ExternalCommon.DTOs;
using Customs.Infrastructure.Tasks.ExternalProxy;
using MalamTeam.Infrastructure.GeneralServices.BaseLayer;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public class TasksServiceAdapter : BaseServiceAdapter<ITasksExternalProxy>, ITasksServiceAdapter
    {
        public LatestUserHandlingEntityTasksResult GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilter filter)
        {
            return ExternalProxy.GetLatestUserHandlingEntityTasksWithTaskUnification(filter);
        }

        public List<LatestUserHandlingUserForTaskTypeResult> GetLatestUserHandlingUserForTaskType(LatestUserHandlingUserForTaskTypeFilter filter)
        {
            return ExternalProxy.GetLatestUserHandlingUserForTaskType(filter);
        }

        public List<TaskDetailsDTO> GetTaskDetailsDTOForEntityTasks(LatestUserHandlingEntityTasksFilter filter)
        {
            return ExternalProxy.GetTaskDetailsDTOForEntityTasks(filter);
        }

        public List<IsTaskExistResultDTO> IsTaskExist(IsTaskExistFilter filter)
        {
            return ExternalProxy.IsTaskExist(filter);
        }

        public bool IsTaskExistsOnEntity(VirtualEntity virtualEntity, int taskTypeID)
        {
            return ExternalProxy.IsTaskExistsOnEntity(virtualEntity, taskTypeID);
        }
    }
}
