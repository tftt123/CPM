/* eslint-disable @typescript-eslint/no-explicit-any */
import request from './request'

export const opportunityApi = {
  list: (params: any) => request.get('/quotation/opportunities', { params }),
  getById: (id: number) => request.get(`/quotation/opportunities/${id}`),
  create: (data: any) => request.post('/quotation/opportunities', data),
  update: (id: number, data: any) => request.put(`/quotation/opportunities/${id}`, data),
  updateStage: (id: number, stage: string) => request.patch(`/quotation/opportunities/${id}/stage`, { stage }),
  delete: (id: number) => request.delete(`/quotation/opportunities/${id}`)
}

export const quotationApi = {
  list: (params: any) => request.get('/quotation/quotations', { params }),
  getById: (id: number) => request.get(`/quotation/quotations/${id}`),
  create: (data: any) => request.post('/quotation/quotations', data),
  update: (id: number, data: any) => request.put(`/quotation/quotations/${id}`, data),
  delete: (id: number) => request.delete(`/quotation/quotations/${id}`),
  submitForApproval: (id: number) => request.post(`/quotation/quotations/${id}/submit`),
  processApproval: (id: number, data: any) => request.post(`/quotation/quotations/${id}/approve`, data),
  getApprovalRecords: (id: number) => request.get(`/quotation/quotations/${id}/approval-records`),
  getApprovalSteps: (id: number) => request.get(`/quotation/quotations/${id}/approval-steps`),
  canApprove: (id: number) => request.get(`/quotation/quotations/${id}/can-approve`),
  getApprovalForecast: (id: number) => request.get(`/quotation/quotations/${id}/approval-forecast`)
}

export const approvalApi = {
  getTemplates: (moduleType?: string) => request.get('/approval/templates', { params: { moduleType } }),
  getTemplateById: (id: number) => request.get(`/approval/templates/${id}`),
  createTemplate: (data: any) => request.post('/approval/templates', data),
  updateTemplate: (id: number, data: any) => request.put(`/approval/templates/${id}`, data),
  deleteTemplate: (id: number) => request.delete(`/approval/templates/${id}`)
}

export const fileApi = {
  getFiles: (moduleType: string, businessId: number) => request.get('/fileupload/files', { params: { moduleType, businessId } }),
  deleteFile: (id: number) => request.delete(`/fileupload/${id}`)
}
