export interface Notification {
  id: number;
  title: string;
  message: string;
  type: number;
  typeName: string;
  relatedEntityType?: string;
  relatedEntityId?: number;
  isRead: boolean;
  createdAt: string;
}
