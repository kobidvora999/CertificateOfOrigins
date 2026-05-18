using Customs.Infrastructure.Tasks.ExternalCommon;
using Customs.Infrastructure.Tasks.ExternalCommon.DTOs;
using MalamTeam.Infrastructure.GeneralServices.CommonBase.Base_Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customs.CRM.CertificateOfOrigins.BL.ServiceAdapters
{
    public interface ITasksServiceAdapter
    {

        List<LatestUserHandlingUserForTaskTypeResult> GetLatestUserHandlingUserForTaskType(LatestUserHandlingUserForTaskTypeFilter filter);
        List<TaskDetailsDTO> GetTaskDetailsDTOForEntityTasks(LatestUserHandlingEntityTasksFilter filter); // by entity & taskTypeId
        bool IsTaskExistsOnEntity(VirtualEntity virtualEntity, int taskTypeID);
        List<IsTaskExistResultDTO> IsTaskExist(IsTaskExistFilter filter);
        LatestUserHandlingEntityTasksResult GetLatestUserHandlingEntityTasksWithTaskUnification(LatestUserHandlingEntityTasksFilter filter);

    }
}
