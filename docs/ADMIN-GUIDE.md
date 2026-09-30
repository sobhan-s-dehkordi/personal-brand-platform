# Editing and operations

## Content editor

The admin uses self-hosted Quill 2.0.3 (BSD license in `wwwroot/lib/quill/LICENSE`). Article, project, course and workshop descriptive text; lesson/session text; page content; payment instructions; support bold, italic, underline, strike, lists, quotes, links, code blocks and undo/redo. Titles, slugs, metadata and operational fields remain plain text.

The toolbar and editor format allowlist exclude font, size, color, headings, image and video embeds. Pasted content follows the same restrictions. Server rendering independently strips CSS, classes, event handlers, unsafe URLs and active HTML. Public typography stays under site CSS control.

Existing Markdown remains supported. Rich content uses the `pb-html:` storage prefix. Opening a record does not rewrite existing Markdown; editing a rich field converts that field to restricted HTML. Summaries render as rich content on pages/cards and plain text in SEO metadata. Keep descriptive summaries concise.

## Bilingual editing and visibility

The administration interface is English and left-to-right. Articles, projects, courses, workshops, lessons, and sessions use a single form with English and Persian panels. Persian content fields are right-to-left. Complete the title and body in both languages, even when one language is hidden, and use **Save both languages**. Metadata, URLs, related courses, tags, screenshots, schedules, and other details belong to their own language panel.

Each language remains a separate database record. Translation groups are assigned automatically; administrators never enter an ID. Both records save in one transaction. The content list has one entry per pair, using its English title, and searches titles in either language. Open that English entry to edit either language.

**Show on the English/Persian website** controls each record independently. Publication status still applies: a visible draft is not public. Hidden content is excluded from listings, home cards, direct public URLs, sitemap, translation links, and new registrations. Existing private reservation links and records remain available.

Workshop capacity, price, schedule, reservations, and cancellation remain independent for each language. The dashboard and workshop list provide separate reservation links for English and Persian. Hiding a workshop does not cancel its reservations.

## Homepage and fixed interface text

**Homepage and identity** edits both languages together: name, biography, hero heading, introductory copy, newsletter copy, footer copy, contact details, and social URLs. **Pages and settings** edits page bodies and payment details. Navigation labels, buttons, form labels, system messages, and empty-section messages are fixed bilingual interface text; old database overrides for them are ignored.

## Existing content and migration

Apply the checked-in migrations before running the updated applications. Existing IDs, content, visibility, and reservations are preserved. Existing child records are paired by their position within already paired parents. If an old record has no counterpart, the migration adds a hidden draft marked as needing translation; complete it in the bilingual editor before saving. It never automatically publishes a copied translation.

## Password

Open **Change password**. Enter the current password and confirm the new password. Identity checks the current password and applies the existing password policy (12+ characters with upper/lowercase, digit and symbol). The current session refreshes; other sessions are invalidated on their next security-stamp validation. Password changes do not reset on restart.

## Card-to-card purchase flow

1. Publish the workshop, open registration, set its dates, capacity, amount/currency and joining details (Platform).
2. In **Settings → Card-to-card payment**, enter the bank, cardholder, displayed card number and instructions for each language.
3. A participant submits their contact details. A transaction reserves a seat temporarily and displays a private payment link; an email with the link and an admin Telegram notification are queued.
4. The participant pays outside the site and uploads a valid receipt image with a tracking number before the hold expires. The site stores the image privately and queues receipt acknowledgement and admin notification. It does not contact a bank or verify transfers automatically.
5. In **Reservations**, open the reservation, inspect the private receipt, and confirm/reject/cancel. Confirmation queues an email with joining details and the session schedule; each review also queues an admin Telegram status notification. Internal notes are not included in participant notifications.
6. The participant can revisit the private link for current status. Confirmed and submitted reservations consume capacity; expired pending holds release it.

## Actual email and Telegram delivery

Both adapters and the persistent retry queue are implemented. Live delivery requires external credentials, which are intentionally absent from source. Configure the Admin application's secrets/environment:

| Setting | Purpose |
| --- | --- |
| `Email__Host`, `Email__Port` | SMTP server and port |
| `Email__From` | Authorized sender address |
| `Email__Username`, `Email__Password` | SMTP credentials |
| `Email__EnableSsl` | TLS setting for the provider |
| `Telegram__BotToken`, `Telegram__ChatId` | Bot token and administrator destination chat |
| `Site__BaseUrl` (both apps) | Public HTTPS address used in private links |

Use the deployment secret manager or local user secrets; do not commit credentials. Ensure the bot can send to the chosen chat. Telegram notifications go to the configured administrator chat, not automatically to participant usernames. Receipt photos are accessed only through the authenticated admin panel.

The Admin service must remain running for dispatch and expiry processing. **Notifications** shows configured/missing integrations and the latest 100 queued/sent messages. Configuration is not a delivery test. Failed messages retry with backoff up to ten attempts; use **Retry** after fixing configuration. Outbox delivery is at-least-once: a process failure after sending but before recording success may duplicate a notification.

Validation: automated tests cover protected admin/password operations, fixed bilingual interface text and atomic bilingual editing, rich-text sanitization, and real SQL reservation → private image upload → admin confirmation → notification queue creation. Real external SMTP/Telegram delivery still requires configured credentials and a live delivery check.
