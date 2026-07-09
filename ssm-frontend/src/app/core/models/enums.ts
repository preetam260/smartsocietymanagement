export type UserRole = 'Admin' | 'Resident' | 'SecurityStaff' | 'Owner';

export type BillingStatus = 'Unpaid' | 'Paid' | 'Overdue' | 'Disputed' | 'Processing';

export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed' | 'Held' | 'Expired';

export type ComplaintStatus = 'Open' | 'Resolved' | 'Closed' | 'InProgress' | 'Rejected';

export type VisitorStatus = 'Pending' | 'Approved' | 'CheckedIn' | 'CheckedOut' | 'Expired' | 'Denied';
