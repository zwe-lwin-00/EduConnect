export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    REFRESH: '/auth/refresh',
    LOGOUT: '/auth/logout',
    CHANGE_PASSWORD: '/auth/change-password'
  },
  ADMIN: {
    ONBOARD_TEACHER: '/admin/onboard-teacher',
    TEACHERS: '/admin/teachers',
    PARENTS: '/admin/parents',
    VERIFY_TEACHER: '/admin/verify-teacher',
    REJECT_TEACHER: '/admin/reject-teacher'
  },
  TEACHER: {
    AVAILABILITY: '/teacher/availability',
    PROFILE: '/teacher/profile',
    SESSIONS: '/teacher/sessions'
  },
  STUDENT: {
    LIST: '/student',
    BY_PARENT: '/student/by-parent',
    WALLET: '/student/wallet'
  },
  CONTRACT: {
    CREATE: '/contract',
    APPROVE: '/contract/approve',
    CANCEL: '/contract/cancel',
    LIST: '/contract'
  },
  ATTENDANCE: {
    CHECK_IN: '/attendance/check-in',
    CHECK_OUT: '/attendance/check-out',
    SESSIONS: '/attendance/sessions'
  }
} as const;
