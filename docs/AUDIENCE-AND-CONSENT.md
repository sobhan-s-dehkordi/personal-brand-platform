# Audience and consent

Contact is the practical person record. Email is trimmed and normalized case-insensitively; a unique SQL index plus an email-scoped transaction lock prevents concurrent duplicate creation. The application does not attempt phone/social-account identity resolution.

Anonymous requests can create a Contact but cannot overwrite an existing person's name, phone, email, or Telegram identity merely by supplying that email. They update activity time. A course/contact uniqueness constraint prevents duplicate enrollment; repeating the operation returns the same friendly success experience.

CourseEnrollment records course, contact, language, source, timestamp, consent-at-enrollment snapshot, and policy version. WorkshopReservation references the same Contact and stores its own transaction/payment terms. Neither activity grants promotional permission automatically.

ConsentRecord preserves purpose, grant time, source, policy version, and revocation time. The single optional checkbox clearly covers educational updates, articles/newsletter, and workshop announcements. Selecting it writes purpose-specific records and activates a Subscription. It is never preselected. The combined checkbox can later be replaced with separate purpose choices without changing the schema.

Subscription carries a cryptographically random 256-bit unsubscribe token. A GET shows a confirmation form so mail scanners cannot silently unsubscribe; a CSRF-protected POST revokes current promotional consents and marks the subscription Unsubscribed. No public account is needed. Operational enrollment, reservation, payment, audit, and logistics records remain intact.

Operational messages are queued transactionally. They do not imply marketing consent. Marketing campaign delivery is not implemented: any later provider must check both the subscription state and current purpose consent, and include an unsubscribe URL. Opt-in confirmation messages include that URL today.

Admin has paginated contact, enrollment, subscriber, and reservation views with contact histories. Access is restricted to Administrator. Contact details and receipt contents are not copied to Telegram notifications. A retention/deletion policy and verified owner contact details must be set before public deployment. No claim of universal legal compliance is made by the sample policy.

