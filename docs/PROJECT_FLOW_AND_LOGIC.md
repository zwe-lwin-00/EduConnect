# EduConnect – Project flow and logic

This document describes the end-to-end flow and logic so that features stay consistent and nothing is “out of logic.”

---

## 1. Intended flow (admin → teacher/parent)

1. **People**
   - Create **Teachers** (onboard), **Parents**, and **Students** (linked to a parent).

2. **Subscription (Add paid months)**
   - Admin goes to **Subscription → Add paid months** (payments page).
   - Clicks **Add subscription**: chooses a **Student**, **Type** (One-to-one or Group), and **Duration** (months).
   - This creates a **Subscription** (e.g. SUB-xxx) valid until the end of the chosen period. Only **active** subscriptions with `subscriptionPeriodEnd >= today` are used for access.

3. **Teaching – One-To-One**
   - Admin goes to **Teaching → One-To-One**.
   - Clicks **New One-To-One class**.
   - Selects **Student (One-to-one subscription)** – only students with an active **One-to-one** subscription appear.
   - Selects **Teacher**, optionally **Start/End date**, and **Days of week** (Mon–Sun checkboxes) and **Start/End time**.
   - This creates a **ContractSession** (1:1 class) linked to that subscription. The contract stores **DaysOfWeek**, **StartTime**, **EndTime** so the teacher (and parent) calendar can show **upcoming** and **completed** sessions.

4. **Teaching – Group**
   - Admin goes to **Teaching → Group**.
   - **Create group class**: **Name**, **Teacher**, **Days of week**, **Start/End time**.
   - Then **Enrollments**: enroll students either by **Group subscription** or by **One-to-one contract** (same teacher). Each enrollment stores either `SubscriptionId` (group) or `ContractId` (1:1); access is checked from that subscription or contract.

5. **Attendance**
   - **One-to-one**: Teacher uses **Sessions** (or check-in/check-out) to start/end a session → creates **AttendanceLog** for that contract. Teacher calendar shows **completed** from logs and **upcoming** from contract schedule.
   - **Group**: Teacher starts/ends a **group session** (check-in/check-out for the group class) → creates **GroupSession**. Teacher calendar shows completed group sessions and upcoming slots from the group class schedule.

6. **Calendars**
   - **Teacher calendar (month)**: Shows **one-to-one** (from contracts + attendance logs) and **group** (from GroupSessions + GroupClass schedule), with **DateYmd** for correct day matching, and **Completed** / **Upcoming** / **Holiday**.
   - **Parent calendar (month, per student)**: Shows the same for that student: **one-to-one** (contracts + logs) and **group** (enrollments with access → group class schedule + GroupSessions). Group rows show “Group: &lt;class name&gt;”.

---

## 2. Data and access rules

- **ContractSession (1:1)**
  - `SubscriptionId` set → access from **Subscription** (active + `SubscriptionPeriodEnd >= now`).
  - `SubscriptionId` null → legacy: access from **SubscriptionPeriodEnd** on the contract.
  - Teacher calendar uses **Active** contracts with **DaysOfWeek** and **StartTime** (no subscription filter); parent calendar uses the same contracts but only when **HasActiveAccess** (subscription/period valid).

- **GroupClassEnrollment**
  - Enrolled by **Group subscription** → `SubscriptionId` set; access when subscription is active.
  - Enrolled by **One-to-one contract** → `ContractId` set; access when contract has active access.
  - Parent month calendar only includes group sessions for enrollments that **HasGroupEnrollmentAccess** (subscription or contract valid).

- **Schedule**
  - **DaysOfWeek**: comma-separated ISO 1–7 (1=Monday … 7=Sunday). Validated in backend (ScheduleValidation).
  - **StartTime / EndTime**: optional; if both set, end must be after start. Teacher/parent calendars only show sessions when **DaysOfWeek** and **StartTime** are set.

---

## 3. Consistency checks (what was verified/fixed)

- **One-to-one class form**: Has **Days of week** (checkboxes like group class) and **Start/End time** so that teacher and parent calendars can show upcoming 1:1 sessions.
- **Teacher calendar**: Shows both **one-to-one** (logs + scheduled from contract) and **group** (GroupSessions + scheduled from GroupClass). Uses **DateYmd** so sessions land on the correct calendar day.
- **Parent calendar**: Now includes **group** sessions (upcoming + completed) for the student, using enrollments with access; display uses “Group: &lt;name&gt;” and same Completed/Upcoming logic as teacher.
- **Contracts table**: **Schedule** column shows “Days · Start–End” (e.g. Mon, Wed · 09:00–10:00) so admin can see at a glance that the 1:1 class has a weekly schedule.
- **Subscription flow**: Add paid months first (One-to-one or Group) → then create One-To-One class (with 1:1 subscription) or Group class and enroll (Group subscription or 1:1 contract). No logic for creating a class without the correct subscription/contract type.

---

## 4. Sidebar and naming

- **Subscription** section: **Add paid months** → opens the **Subscriptions** page (list + “Add subscription”). Same flow; label is “Add paid months”, page title is “Subscriptions”.
- **Teaching**: **One-To-One** (contracts), **Group** (group classes), **Attendance** (admin oversight).

---

## 5. Optional / future

- **Edit contract**: Currently no UI to edit DaysOfWeek/StartTime/EndTime after creation; only cancel. Adding an edit would keep calendar in sync if admin changes schedule.
- **Parent week calendar**: GetSessionsForStudentWeekAsync currently returns only completed 1:1 logs; it could be extended to include scheduled 1:1 and group (like month) for consistency.
