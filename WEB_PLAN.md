# Plan: The Green Memoir - Web Application

## Context

Game "The Green Memoir" là game nông trại 2D trả phí, cần một website phục vụ: giới thiệu game, đăng ký/đăng nhập, thanh toán qua Sepay (QR chuyển khoản ngân hàng), tải game, và quản trị. Game hiện dùng Firebase Auth + Realtime Database (project `field-12d9d`). Web app phải tích hợp cùng Firebase project để đồng bộ trạng thái thanh toán giữa web và game.

Logo: `Assets/Image/04306d64-faac-4114-b77b-ba300e092e13.png`

---

## Tech Stack

| Layer | Công nghệ | Lý do |
|-------|-----------|-------|
| Framework | **Next.js 14+ (App Router, SSR)** | Server Components cho SSR nhanh, file-based routing, API routes built-in |
| Language | **TypeScript** | Type safety, Firebase SDK hỗ trợ tốt |
| Database | **Firestore** (web data) + **RTDB** (game data) | Cùng Firebase project, không cần DB riêng, deploy Vercel được |
| Auth | **Firebase Admin SDK** + session cookie | Dùng chung Firebase Auth với game |
| Payment | **Sepay** (QR VietQR + Webhook) | QR chuyển khoản, tự động nhận diện |
| Styling | **Tailwind CSS** | Nhanh, custom theme dễ |
| i18n | **next-intl** | Vietnamese (chính) + English |
| Deploy | **Vercel** (web) + **Cloud Storage / VPS** (game file) | Vercel free tier, game file lưu external |

---

## Database (Firestore Collections)

- **RTDB** (`Users/{uid}`): Game data — giữ nguyên (Name, Gold, Diamond, MapInGame, Version) + thêm `hasPurchased`
- **Firestore**: Web commerce data — orders, download tokens, game versions, stats

```
Firestore Collections:

── webUsers/{uid}                    // uid = Firebase Auth UID
   ├── email: string
   ├── displayName: string | null
   ├── role: "user" | "admin"
   ├── hasPurchased: boolean
   ├── createdAt: Timestamp
   └── updatedAt: Timestamp

── orders/{autoId}
   ├── userId: string               // Firebase Auth UID
   ├── userEmail: string            // Denormalized cho admin query
   ├── orderCode: string            // Unique, dùng làm nội dung CK (vd: "GM-A1B2C3")
   ├── amount: number               // VND (49000)
   ├── status: "pending" | "paid" | "failed" | "expired"
   ├── sepayId: number | null       // Sepay transaction id (deduplicate)
   ├── referenceCode: string | null // Mã tham chiếu ngân hàng
   ├── paidAt: Timestamp | null
   ├── expiresAt: Timestamp         // Hết hạn sau 30 phút
   └── createdAt: Timestamp

── downloadTokens/{autoId}
   ├── userId: string
   ├── token: string                // Crypto random, unique
   ├── versionId: string
   ├── expiresAt: Timestamp         // Hết hạn 24h
   ├── usedAt: Timestamp | null
   └── createdAt: Timestamp

── gameVersions/{autoId}
   ├── versionNumber: string        // "0.1.0-alpha"
   ├── displayName: string
   ├── changelog: string            // Markdown
   ├── downloadUrl: string          // Firebase Storage hoặc external URL
   ├── fileSize: number             // Bytes
   ├── checksum: string             // SHA256
   ├── isLatest: boolean
   ├── isActive: boolean
   └── createdAt: Timestamp

── siteStats/{date}                 // doc id = "2026-03-15"
   ├── pageViews: number
   └── uniqueVisitors: number
```

**Firestore Indexes cần tạo:**
- `orders`: `userId + createdAt` (DESC) — query orders của 1 user
- `orders`: `status + createdAt` (DESC) — admin filter theo status
- `orders`: `orderCode` — webhook lookup
- `downloadTokens`: `token` — download validation

---

## Sepay Integration (QR + Webhook tự động)

### Flow thanh toán

```
User bấm "Mua Game"
  → Server tạo Order với orderCode unique (vd: "GM-A1B2C3")
  → Server trả về QR URL: https://qr.sepay.vn/img?acc=ACCOUNT&bank=BANK&amount=49000&des=GM-A1B2C3
  → Hiển thị QR + thông tin chuyển khoản + countdown 30 phút
  → User quét QR, chuyển khoản với nội dung "GM-A1B2C3"
  → Sepay nhận giao dịch, gửi webhook POST đến server
  → Server nhận webhook, parse trường `content` để tìm orderCode
  → So khớp → cập nhật Order status = "paid", user hasPurchased = true
  → Đồng thời ghi vào Firebase RTDB: Users/{uid}/hasPurchased = true
  → Frontend polling /api/payment/check-status mỗi 5 giây
  → Phát hiện "paid" → chuyển sang trang thành công
```

### Webhook handler (`POST /api/payment/webhook`)

```
Sepay gửi POST với header: "Authorization: Apikey <SEPAY_API_KEY>"

Body:
{
  "id": 92704,              // Unique transaction ID - dùng để deduplicate
  "gateway": "Vietcombank",
  "transactionDate": "2023-03-25 14:02:37",
  "accountNumber": "0123499999",
  "code": null,             // Mã thanh toán (nếu Sepay detect được)
  "content": "GM-A1B2C3 chuyen tien",  // Nội dung CK - chứa orderCode
  "transferType": "in",     // "in" = tiền vào
  "transferAmount": 49000,
  "accumulated": 19077000,
  "subAccount": null,
  "referenceCode": "MBVCB.3278907687",
  "description": ""
}

Xử lý:
1. Verify API Key trong header Authorization
2. Check transferType === "in" (chỉ xử lý tiền vào)
3. Check tính duy nhất của trường `id` (deduplicate)
4. Parse `content` hoặc `code` để tìm orderCode (regex match "GM-XXXXXX")
5. Tìm Order trong DB theo orderCode
6. Verify transferAmount >= order.amount
7. Verify order chưa hết hạn và đang "pending"
8. Cập nhật: order.status = "paid", user.hasPurchased = true
9. Ghi Firebase RTDB: Users/{firebaseUid}/hasPurchased = true
10. Respond: { "success": true } với HTTP 200
```

### QR Code embed

```html
<img src="https://qr.sepay.vn/img?acc={BANK_ACCOUNT}&bank={BANK_NAME}&amount=49000&des={ORDER_CODE}" />
```

Danh sách mã ngân hàng: `https://qr.sepay.vn/banks.json`

---

## Project Structure

```
web/                              # Subproject trong repo Moonlit-Garden
├── public/images/                # Logo, screenshots, favicon
├── messages/                     # vi.json, en.json (i18n)
├── src/
│   ├── app/
│   │   ├── layout.tsx            # Root layout
│   │   ├── page.tsx              # Landing page
│   │   ├── (auth)/login|register|forgot-password/
│   │   ├── (user)/profile|purchase|download/
│   │   ├── (admin)/dashboard|users|orders|versions/
│   │   └── api/
│   │       ├── auth/session|logout/
│   │       ├── payment/create-order|webhook|check-status/
│   │       ├── download/generate-token|[token]/
│   │       └── admin/stats|users|orders|versions/
│   ├── lib/
│   │   ├── firebase-admin.ts     # Firebase Admin SDK (Firestore + RTDB + Auth)
│   │   ├── firebase-client.ts    # Firebase Client SDK (browser auth)
│   │   ├── firestore.ts          # Firestore helper functions (CRUD cho collections)
│   │   ├── sepay.ts              # Sepay QR URL builder + webhook verification
│   │   └── auth.ts               # Session cookie management
│   ├── components/
│   │   ├── layout/               # Header, Footer, AdminSidebar
│   │   ├── landing/              # Hero, Screenshots, Features, Pricing
│   │   ├── auth/                 # LoginForm, RegisterForm
│   │   ├── payment/              # QRPayment, OrderStatus, PaymentTimer
│   │   ├── download/             # DownloadCard, Changelog
│   │   ├── admin/                # StatsCards, UserTable, OrderTable
│   │   └── ui/                   # Button, Input, Card, PixelBorder
│   ├── hooks/                    # useAuth, usePaymentStatus, useCountdown
│   └── types/
└── __tests__/
```

---

## Chi tiết UI/UX từng trang

### Bảng màu & Typography

```
Màu chính (từ logo - lá xanh, hoa trắng, đất nâu):
  --green-dark:   #2D5A27   (Nút chính, header)
  --green-main:   #4A8C3F   (Accent, links)
  --green-light:  #7BC96F   (Hover states)
  --green-pale:   #E8F5E3   (Nền section)
  --brown-dark:   #5C3D2E   (Text, borders)
  --brown-light:  #C4A265   (Accent phụ)
  --cream:        #FFF8E7   (Nền trang - giấy cũ)
  --cream-dark:   #F5E6C8   (Nền card)
  --gold:         #DAA520   (Giá tiền, premium)
  --border:       #D4C5A9   (Viền tre/gỗ)

Font:
  Display: "Cherry Bomb One" (tên game, tiêu đề lớn - match font trong game)
  Heading: "Playfair Display" (tiêu đề trang)
  Body:    "Nunito" (nội dung - hỗ trợ tiếng Việt tốt)
```

### Header (chung tất cả trang)
- Fixed top, 64px, nền `--cream` với blur, viền dưới `--border`
- Trái: Logo + "The Green Memoir" (Cherry Bomb One)
- Giữa (desktop): Trang Chủ | Tính Năng | Mua Game | Tải Về
- Phải: Toggle VI/EN + Đăng nhập/Đăng ký (hoặc avatar dropdown nếu đã login)
- Mobile: Hamburger menu trượt từ phải

### Footer (chung tất cả trang)
- Nền `--brown-dark`, viền trên pixel art cỏ/đất
- 3 cột: Logo+mô tả | Links (Chính sách, Điều khoản, Liên hệ) | Social (FB, Discord)
- "Made with ♥ in Vietnam"

---

### Trang 1: Landing Page (`/`)

**Section 1 — Hero (full viewport)**
- Background: Pixel art farm landscape (parallax) với hiệu ứng lá bay (CSS animation)
- Logo game 300px, glow animation nhẹ
- Tagline: "Ký ức xanh của miền quê Việt Nam"
- Subtitle: "Game nông trại 2D với văn hóa Việt Nam"
- 2 nút CTA:
  - Primary (xanh): "Mua Game — 49,000₫"
  - Secondary (outlined): "Xem Trailer ▶"
- Dưới cùng: "Hỗ trợ Windows 10/11 | Early Access"

**Section 2 — Trailer**
- Nền `--cream-dark`, video YouTube 16:9, khung gỗ pixel art
- Mô tả game 1 đoạn bên dưới

**Section 3 — Screenshots**
- Gallery cuộn ngang (mobile), grid 2x2 (desktop)
- Mỗi ảnh có khung gỗ, hover zoom 1.05, click mở lightbox

**Section 4 — Tính năng**
- Nền `--green-pale` với pattern lá
- Grid 3 cột (desktop), 1 cột (mobile)
- 6 card: Trồng trọt, Văn hóa VN, Nhân vật nông dân, Ngày đêm, Kinh tế, Cloud Save
- Mỗi card: Icon pixel 48px + tên + mô tả 2 dòng

**Section 5 — Cấu hình yêu cầu**
- 2 cột card (Tối thiểu | Khuyến nghị), style bảng gỗ
- OS, CPU, RAM, GPU, Storage

**Section 6 — Lộ trình**
- Timeline dọc với dây tre/nho nối
- Mỗi milestone: Ngày, version, tiêu đề, badge trạng thái (Xong/Đang làm/Dự kiến)

**Section 7 — CTA mua game**
- Card lớn trung tâm: Logo + giá "49,000₫" (vàng gold) + nút "Mua Ngay"
- Trust signals: "Thanh toán an toàn", "Cập nhật miễn phí"

**Responsive:** Desktop >1024px full layout | Tablet 768-1024px 2 cột | Mobile <768px 1 cột, hamburger nav

---

### Trang 2: Đăng ký (`/register`)
- Card trung tâm 440px, nền trắng, viền pixel
- Logo 80px + "Đăng Ký Tài Khoản"
- Fields: Email, Mật khẩu (≥6 ký tự), Xác nhận mật khẩu, Tên hiển thị (optional)
- Nút "Tạo Tài Khoản" (xanh, full width)
- Divider "--- hoặc ---" + Google Sign-In
- Link: "Đã có tài khoản? Đăng nhập"

### Trang 3: Đăng nhập (`/login`)
- Cùng layout card
- Fields: Email, Mật khẩu (toggle show/hide)
- Checkbox "Nhớ mật khẩu"
- Nút "Đăng Nhập"
- Link "Quên mật khẩu?" + Google Sign-In
- Link: "Chưa có tài khoản? Đăng ký"

### Trang 4: Quên mật khẩu (`/forgot-password`)
- Card đơn giản: Email field + nút "Gửi Liên Kết"
- Success: Icon check xanh + "Đã gửi! Kiểm tra hộp thư email."

---

### Trang 5: Hồ sơ (`/profile`) — Cần đăng nhập

- **Sidebar trái:** Avatar (chữ cái đầu), tên, email (masked), ngày tham gia, badge "Đã Mua"/"Chưa Mua"
- **Nội dung chính:**
  - Game đã mua: Card The Green Memoir + version + ngày mua + nút "Tải Về" (hoặc "Mua Ngay" nếu chưa mua)
  - Lịch sử giao dịch: Bảng (Ngày, Mã đơn, Số tiền, Trạng thái)
  - Cài đặt: Đổi tên hiển thị, đổi mật khẩu, đăng xuất

---

### Trang 6: Mua Game (`/purchase`) — Cần đăng nhập

**Bước 1 — Xác nhận đơn hàng:**
- Card: Ảnh game/logo + "The Green Memoir - Early Access" + giá 49,000₫
- 3 bullet tính năng chính
- Nút "Thanh Toán" + link "Quay lại"

**Bước 2 — QR Thanh toán (CORE FEATURE):**
- QR Code lớn 280x280: `<img src="https://qr.sepay.vn/img?acc=...&bank=...&amount=49000&des=GM-XXXXXX">`
- Thông tin chuyển khoản bên dưới:
  - Ngân hàng: [tên]
  - Số tài khoản: [số]
  - Chủ tài khoản: [tên]
  - Số tiền: 49,000₫
  - Nội dung CK: GM-XXXXXX (highlight, có nút copy)
- Countdown: "Còn lại 29:45" (đỏ khi <2 phút)
- Trạng thái: "Đang chờ thanh toán..." + spinner
- Auto-polling `/api/payment/check-status` mỗi 5 giây
- Nút "Tôi Đã Thanh Toán" (trigger check ngay)

**Bước 3 — Kết quả:**
- Thành công: Checkmark xanh animation + "Thanh toán thành công!" + nút "Tải Game Ngay"
- Thất bại/Hết hạn: X đỏ + "Thanh toán không thành công" + nút "Thử Lại"

---

### Trang 7: Tải Game (`/download`) — Cần đăng nhập + đã mua

- Nếu chưa mua: redirect `/purchase`
- **Download Card:**
  - Logo + tên game
  - Version badge: "v0.1.0-alpha"
  - Dung lượng: "450 MB"
  - Nút "Tải Về cho Windows" (xanh, lớn, icon download)
  - Expandable: SHA256 checksum
- **Changelog:** Accordion từng version, latest mở sẵn
- **Hướng dẫn cài đặt:** 4 bước (giải nén → chạy exe → đăng nhập → chơi)

---

### Trang 8-11: Admin Panel (`/admin/*`) — Cần role admin

**Layout chung:** Sidebar trái (Dashboard, Users, Orders, Versions) + nội dung chính

**Dashboard (`/admin/dashboard`):**
- 4 stat cards: Tổng Users, Doanh thu, Tổng đơn hàng, Lượt truy cập hôm nay
- Biểu đồ doanh thu 30 ngày (bar chart CSS)
- Bảng 10 đơn hàng gần nhất + 10 user mới nhất

**Users (`/admin/users`):**
- Bảng searchable/sortable: Email, Tên, Firebase UID, Đã mua (badge), Ngày ĐK
- Pagination 20/trang

**Orders (`/admin/orders`):**
- Filter theo status (tất cả/pending/paid/failed)
- Bảng: Mã đơn, Email, Số tiền, Status (badge), Ngày tạo, Ngày TT
- Nút xác nhận thủ công (edge cases)
- Export CSV

**Versions (`/admin/versions`):**
- Danh sách version với toggle active/latest
- Form thêm version: số version, tên, changelog (markdown), upload file

---

## API Routes

### Auth
| Method | Route | Mô tả | Auth |
|--------|-------|-------|------|
| POST | `/api/auth/session` | Đổi Firebase ID token → session cookie | Firebase token |
| POST | `/api/auth/logout` | Xóa session cookie | Session |
| GET | `/api/auth/me` | Lấy thông tin user hiện tại | Session |

### Payment
| Method | Route | Mô tả | Auth |
|--------|-------|-------|------|
| POST | `/api/payment/create-order` | Tạo order mới, trả QR URL | Session (user) |
| POST | `/api/payment/webhook` | Nhận webhook từ Sepay | Sepay API Key |
| GET | `/api/payment/check-status?orderId=x` | Polling trạng thái thanh toán | Session (user) |

### Download
| Method | Route | Mô tả | Auth |
|--------|-------|-------|------|
| POST | `/api/download/generate-token` | Tạo token download 1 lần | Session (purchased) |
| GET | `/api/download/[token]` | Stream file game | Token in URL |

### Admin
| Method | Route | Mô tả | Auth |
|--------|-------|-------|------|
| GET | `/api/admin/stats` | Thống kê dashboard | Session (admin) |
| GET | `/api/admin/users` | Danh sách user (paginated) | Session (admin) |
| GET | `/api/admin/orders` | Danh sách order (filtered) | Session (admin) |
| PATCH | `/api/admin/orders/[id]/confirm` | Xác nhận thủ công | Session (admin) |
| GET/POST | `/api/admin/versions` | CRUD game versions | Session (admin) |

---

## Security

1. **Auth flow:** Firebase Client SDK login → get ID token → POST `/api/auth/session` → server verify bằng Firebase Admin SDK → tạo httpOnly session cookie (5 ngày)
2. **Webhook security:** Verify header `Authorization: Apikey <SEPAY_API_KEY>`, check unique `id` (deduplicate), verify `transferAmount >= order.amount`, verify order chưa expired
3. **Download protection:** Token 1 lần, hết hạn 24h, stream file qua API (không expose path thực)
4. **Rate limiting:** Auth endpoints 5 req/phút/IP, payment 3 req/phút/user
5. **Input validation:** Zod schemas cho tất cả API inputs
6. **Game-side sync:** Khi payment thành công, ghi `Users/{uid}/hasPurchased = true` vào Firebase RTDB → game đọc trường này khi login

---

## Critical Files cần tham khảo khi implement

- [User.cs](Assets/Scripts/Entities/User.cs) — Data model Firebase RTDB, cần thêm trường `hasPurchased`
- [google-services.json](Assets/google-services.json) — Firebase config (project `field-12d9d`)
- [FirebaseTransactionManager.cs](Assets/Scripts/Firebase/FirebaseTransactionManager.cs) — Path structure `Users/{userId}`
- [FirebaseLoginManager.cs](Assets/Scripts/FirebaseLoginManager.cs) — Auth flow hiện tại của game
- [FLOW.md](FLOW.md) — Business requirements đầy đủ
- [Logo PNG](Assets/Image/04306d64-faac-4114-b77b-ba300e092e13.png) — Logo game cho web

---

## Deployment

**Web app:** Vercel (free tier)
- Connect GitHub repo, auto-deploy on push
- Env vars: Firebase service account JSON, Sepay API key, session secret
- Custom domain: trỏ DNS về Vercel

**Game file download:** 2 options:
- **Firebase Storage** (đơn giản nhất): Upload game ZIP lên Firebase Storage, dùng signed URL có thời hạn
- **VPS / Cloud Storage**: Nếu file lớn hoặc cần bandwidth cao

**Backup:** Firebase tự quản lý (Firestore + RTDB đều có automatic backup)

---

## Implementation Phases

### Phase 1 (Tuần 1-2): Nền tảng
- Init Next.js + Firebase Admin SDK (Firestore + RTDB + Auth)
- Firestore collections setup + helper functions
- Auth flow (register, login, session cookies)
- Layout (Header, Footer, theme Tailwind)

### Phase 2 (Tuần 2-3): Landing + User pages
- Landing page (7 sections)
- Profile page
- i18n (vi/en)

### Phase 3 (Tuần 3-4): Thanh toán Sepay
- Tạo order + QR VietQR
- Webhook receiver + verify
- Payment UI (QR, polling, countdown)
- Ghi Firebase RTDB `hasPurchased`
- Test e2e trên `my.dev.sepay.vn`

### Phase 4 (Tuần 4-5): Download
- Token generation + file streaming
- Download page UI + changelog
- Cập nhật game code đọc `hasPurchased`

### Phase 5 (Tuần 5-6): Admin + Polish
- Admin dashboard, tables, version management
- Stats tracking
- Security hardening, rate limiting
- Testing

---

## Verification

1. **Auth:** Đăng ký trên web → đăng nhập trong game bằng cùng tài khoản → thành công
2. **Payment:** Tạo order → quét QR → chuyển khoản test trên `my.dev.sepay.vn` → webhook nhận → status tự động chuyển "paid"
3. **Download:** Sau khi mua → tạo download token → tải file → verify checksum
4. **Game sync:** Mua trên web → `hasPurchased = true` trong Firebase RTDB → game cho phép vào gameplay
5. **Admin:** Xem stats, quản lý user/order, thêm version mới
