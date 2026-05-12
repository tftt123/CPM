import request from './request'

export interface ApprovalTask {
  id: number
  instanceId: number
  stepId: number
  businessType?: string
  businessId: number
  stepName?: string
  templateName?: string
  assigneeId?: number
  assigneeName?: string
  assigneeRole?: string
  status: number
  action?: string
  comment?: string
  dueDate?: string
  createdAt: string
  completedAt?: string
}

export function getMyPendingTasks() {
  return request.get('/approval/my-tasks')
}

export function approveInstance(instanceId: number, data: { comment?: string }) {
  return request.post(`/approval/instances/${instanceId}/approve`, data)
}

export function rejectInstance(instanceId: number, data: { comment?: string }) {
  return request.post(`/approval/instances/${instanceId}/reject`, data)
}
