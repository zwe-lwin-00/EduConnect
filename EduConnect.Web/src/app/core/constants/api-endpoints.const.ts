export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    REFRESH: '/auth/refresh',
    LOGOUT: '/auth/logout',
    CHANGE_PASSWORD: '/auth/change-password'
  },
  NOTIFICATIONS: {
    LIST: '/notifications',
    MARK_READ: (id: number) => `/notifications/${id}/read`
  },
  ADMIN: {
    DASHBOARD: '/admin/dashboard',
    ONBOARD_TEACHER: '/admin/onboard-teacher',
    TEACHERS: '/admin/teachers',
    TEACHER_UPDATE: (id: number) => `/admin/teachers/${id}`,
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
    GROUP_CLASSES: '/admin/group-classes',
    GROUP_CLASS_BY_ID: (id: number) => `/admin/group-classes/${id}`,
    GROUP_CLASS_ENROLLMENTS: (id: number) => `/admin/group-classes/${id}/enrollments`,
    GROUP_CLASS_ENROLL: (id: number) => `/admin/group-classes/${id}/enroll`,
    GROUP_CLASS_UNENROLL: (enrollmentId: number) => `/admin/group-classes/enrollments/${enrollmentId}`,
    ATTENDANCE_TODAY: '/admin/attendance/today',
    ATTENDANCE_OVERRIDE_CHECKIN: (id: number) => `/admin/attendance/${id}/override-checkin`,
    ATTENDANCE_OVERRIDE_CHECKOUT: (id: number) => `/admin/attendance/${id}/override-checkout`,
    ATTENDANCE_ADJUST_HOURS: (id: number) => `/admin/attendance/${id}/adjust-hours`,
    WALLET_CREDIT: '/admin/wallet/credit',
    WALLET_DEDUCT: '/admin/wallet/deduct',
    REPORTS_DAILY: '/admin/reports/daily',
    REPORTS_MONTHLY: '/admin/reports/monthly',
    RESET_TEACHER_PASSWORD: (id: number) => `/admin/teachers/${id}/reset-password`,
    VERIFY_TEACHER: (id: number) => `/admin/teachers/${id}/verify`,
    REJECT_TEACHER: (id: number) => `/admin/teachers/${id}/reject`
  },
  TEACHER: {
    DASHBOARD: '/teacher/dashboard',
    PROFILE: '/teacher/profile',
    PROFILE_ZOOM: '/teacher/profile/zoom',
    STUDENTS: '/teacher/students',
    SESSIONS_TODAY: '/teacher/sessions/today',
    SESSIONS_UPCOMING: '/teacher/sessions/upcoming',
    CALENDAR_WEEK: '/teacher/calendar/week',
    AVAILABILITY: '/teacher/availability',
    CHECK_IN: '/teacher/check-in',
    CHECK_OUT: '/teacher/check-out',
    CHECK_IN_GROUP: '/teacher/check-in/group',
    CHECK_OUT_GROUP: '/teacher/check-out/group',
    GROUP_SESSIONS: '/teacher/group-sessions',
    GROUP_CLASSES: '/teacher/group-classes',
    GROUP_CLASS_BY_ID: (id: number) => `/teacher/group-classes/${id}`,
    GROUP_CLASS_ENROLLMENTS: (id: number) => `/teacher/group-classes/${id}/enrollments`,
    GROUP_CLASS_ENROLL: (id: number) => `/teacher/group-classes/${id}/enroll`,
    GROUP_CLASS_UNENROLL: (enrollmentId: number) => `/teacher/group-classes/enrollments/${enrollmentId}`,
    HOMEWORK: '/teacher/homework',
    HOMEWORK_STATUS: (id: number) => `/teacher/homework/${id}/status`,
    GRADES: '/teacher/grades'
  },
  PARENT: {
    MY_STUDENTS: '/parent/my-students',
    STUDENT_LEARNING_OVERVIEW: (studentId: number) => `/parent/my-students/${studentId}/learning-overview`,
    STUDENT_CALENDAR_WEEK: (studentId: number) => `/parent/my-students/${studentId}/calendar/week`
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
