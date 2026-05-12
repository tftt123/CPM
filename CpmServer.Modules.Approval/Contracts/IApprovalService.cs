using CpmServer.Models;
using CpmServer.Modules.Approval.DTOs;

namespace CpmServer.Modules.Approval.Contracts;

public interface IApprovalService : IApprovalTemplateService, IApprovalInstanceService, IApprovalTaskService, IApprovalActionService
{
}
