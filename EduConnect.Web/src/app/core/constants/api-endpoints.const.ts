export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    REFRESH: '/auth/refresh',
    LOGOUT: '/auth/logout',
    CHANGE_PASSWORD: '/auth/change-password'
  },
  ADMIN: {
    DASHBOARD: '/admin/dashboard',
    ONBOARD_TEACHER: '/admin/onboard-teacher',
    TEACHERS: '/admin/teachers',
    TEACHER_ACTIVATE: (id: number) => `/admin/teachers/${id}/activate`,
    TEACHER_SUSPEND: (id: number) => `/admin/teachers/${id}/activate`,
    PARENTS: '/admin/parents',
    CREATE_PARENT: '/admin/parents',
    STUDENTS: '/admin/students',
    CREATE_STUDENT: '/admin/students',
    STUDENT_SET_ACTIVE: (id: number) => `/admin/students/${id}/set-active`,
    CONTRACTS: '/admin/contracts',
    CONTRACT_BY_ID: (id: number) => `/admin/contracts/${id}`,
    CREATE_CONTRACT: '/admin/contracts',
    CONTRACT_ACTIVATE: (id: number) => `/admin/contracts/${id}/activate`,
    CONTRACT_CANCEL: (id: number) => `/admin/contracts/${id}/cancel`,
    ATTENDANCE_TODAY: '/admin/attendance/today',
    ATTENDANCE_OVERRIDE_CHECKIN: (id: number) => `/admin/attendance/${id}/override-checkin`,
    ATTENDANCE_OVERRIDE_CHECKOUT: (id: number) => `/admin/attendance/${id}/override-checkout`,
    ATTENDANCE_ADJUST_HOURS: (id: number) => `/admin/attendance/${id}/adjust-hours`,
    WALLET_CREDIT: '/admin/wallet/credit',
    WALLET_DEDUCT: '/admin/wallet/deduct',
    REPORTS_DAILY: '/admin/reports/daily',
    REPORTS_MONTHLY: '/admin/reports/monthly',
    VERIFY_TEACHER: (id: number) => `/admin/teachers/${id}/verify`,
    REJECT_TEACHER: (id: number) => `/admin/teachers/${id}/reject`
  },
  TEACHER: {
    DASHBOARD: '/teacher/dashboard',
    PROFILE: '/teacher/profile',
    STUDENTS: '/teacher/students',
    SESSIONS_TODAY: '/teacher/sessions/today',
    SESSIONS_UPCOMING: '/teacher/sessions/upcoming',
    AVAILABILITY: '/teacher/availability',
    CHECK_IN: '/teacher/check-in',
    CHECK_OUT: '/teacher/check-out'
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
