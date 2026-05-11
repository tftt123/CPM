@echo off
chcp 65001 >nul
echo 正在删除 CpmServer 中已迁移的重复文件...
echo.

:: SharedKernel 迁移的文件 (5个)
del "Common\ApiResult.cs"
del "Common\PagedResult.cs"
del "Common\BusinessException.cs"
del "Services\ICurrentUser.cs"
del "Services\CurrentUser.cs"

:: Core 迁移的文件 (25个)
del "Data\CpmDbContext.cs"
del "Models\CrmCustomer.cs"
del "Models\CrmProduct.cs"
del "Models\MfgCategory.cs"
del "Models\MfgEquipment.cs"
del "Models\MfgProcess.cs"
del "Models\MfgSubCategory.cs"
del "Models\QuoOpportunity.cs"
del "Models\QuoQuotation.cs"
del "Models\QuoQuotationItem.cs"
del "Models\SysApprovalCondition.cs"
del "Models\SysApprovalInstance.cs"
del "Models\SysApprovalInstanceTask.cs"
del "Models\SysApprovalRecord.cs"
del "Models\SysApprovalRule.cs"
del "Models\SysApprovalStep.cs"
del "Models\SysApprovalTemplate.cs"
del "Models\SysDept.cs"
del "Models\SysEmailConfig.cs"
del "Models\SysEmailLog.cs"
del "Models\SysEmailTemplate.cs"
del "Models\SysRole.cs"
del "Models\SysUser.cs"
del "Models\SysUserRole.cs"
del "Models\SysUserSite.cs"

:: Approval 模块迁移的文件 (10个)
del "Services\ApprovalService.cs"
del "Services\IApprovalService.cs"
del "Services\Approvers\FixedRoleResolver.cs"
del "Services\Approvers\FixedUserResolver.cs"
del "Services\Approvers\IApproverResolver.cs"
del "Services\Approvers\OrgTreeResolver.cs"
del "Services\Approvers\SubmitterResolver.cs"
del "Controllers\ApprovalController.cs"
del "DTOs\Quotation\ApprovalTemplateDto.cs"
del "DTOs\Quotation\ApprovalActionDto.cs"

:: Quotation 模块迁移的文件 (6个)
del "Services\QuotationService.cs"
del "Services\IQuotationService.cs"
del "Controllers\QuotationController.cs"
del "DTOs\Quotation\OpportunityDto.cs"
del "DTOs\Quotation\QuotationDto.cs"

echo.
echo 清理完成！以下空目录可手动保留或删除:
echo   - Data\
echo   - Models\
echo   - Services\Approvers\
echo.
pause
