PHẦN BÁO CÁO: LUỒNG NGƯỜI DÙNG TỪ WEBSITE ĐẾN GAME
(Phạm vi: Website thanh toán bắt buộc – Game yêu cầu tài khoản hợp lệ)
1.	Tổng quan hệ thống
Dự án được xây dựng theo mô hình game trả phí hoàn toàn. Người dùng không thể tải hoặc chơi game nếu chưa tạo tài khoản và hoàn tất thanh toán trên website. Website đóng vai trò là kênh phân phối chính thức, nơi người dùng đăng ký tài khoản, thanh toán và tải game.
Sau khi cài đặt, game yêu cầu người dùng đăng nhập lại để kiểm tra tính hợp lệ của tài khoản. Chỉ những tài khoản đã thanh toán thành công mới được phép truy cập vào gameplay. Toàn bộ quá trình này được liên kết với hệ thống xác thực và lưu trữ dữ liệu người chơi nhằm đảm bảo tính bảo mật và đồng bộ tiến trình.
2.	Hành trình người dùng tổng thể
Hành trình của một người dùng thông thường diễn ra theo trình tự sau:
Người dùng truy cập website
+ Web cos 
→ Xem thông tin giới thiệu game
→ Đăng ký tài khoản
→ Đăng nhập
→ Thực hiện thanh toán
→ Thanh toán thành công
→ Được mở quyền tải game
→ Tải và cài đặt game
→ Mở game
→ Đăng nhập lại
→ Hệ thống kiểm tra trạng thái thanh toán
→ Tải dữ liệu người chơi
→ Bắt đầu trải nghiệm game
Hành trình này đảm bảo việc kiểm soát quyền truy cập được thực hiện xuyên suốt từ website đến game.
3.	Luồng người dùng trên Website
Trên website, luồng chính gồm ba bước: đăng ký, thanh toán và tải game.
Ở bước đăng ký, người dùng nhập email và mật khẩu để tạo tài khoản. Nếu thông tin hợp lệ, hệ thống tạo tài khoản thành công. Tại thời điểm này, tài khoản chỉ ở trạng thái đã đăng ký nhưng chưa thanh toán.
Sau khi đăng nhập, người dùng thực hiện thanh toán để mua game. Hệ thống chuyển sang cổng thanh toán để xử lý giao dịch.
Nếu giao dịch thất bại, hệ thống hiển thị thông báo và cho phép người dùng thực hiện lại.
Nếu giao dịch thành công, trạng thái tài khoản được cập nhật thành đã thanh toán và quyền tải game được kích hoạt.
Chỉ khi tài khoản ở trạng thái đã thanh toán, nút tải game mới được hiển thị. Cơ chế này nhằm đảm bảo chỉ người đã trả phí mới có thể tải sản phẩm.
4.	Luồng người dùng trong Game
Sau khi tải và cài đặt, người dùng mở game và bắt buộc phải đăng nhập bằng tài khoản đã tạo trên website.
Hệ thống sẽ kiểm tra:
•	Thông tin đăng nhập có chính xác hay không
•	Tài khoản có ở trạng thái đã thanh toán hay không
Nếu đăng nhập sai hoặc tài khoản chưa thanh toán, người dùng không thể vào game.
Nếu tài khoản hợp lệ, hệ thống sẽ tải dữ liệu người chơi như level, tiền, vật phẩm và trạng thái cây trồng. Sau đó người chơi được đưa vào màn hình gameplay.
Việc yêu cầu đăng nhập lại trong game giúp hạn chế tình trạng chia sẻ file cài đặt cho người khác. Nếu không có tài khoản hợp lệ, game sẽ không thể sử dụng.
5.	Luồng hoạt động trong game nông trại
Sau khi đăng nhập thành công, người chơi bắt đầu quản lý nông trại của mình. Gameplay được thiết kế theo dạng vòng lặp đơn giản, dễ hiểu và phù hợp với thể loại mô phỏng nông trại.
Hoạt động cơ bản diễn ra như sau:
Người chơi di chuyển trong khu vực nông trại
→ Chọn hạt giống trong kho
→ Gieo hạt vào ô đất trống
→ Cây phát triển theo thời gian
→ Thu hoạch khi cây trưởng thành
→ Nhận tiền
→ Dùng tiền mua thêm hạt giống hoặc vật phẩm
→ Tiếp tục trồng
Quá trình này lặp lại liên tục và tạo thành vòng chơi chính.
Khi trồng cây, hệ thống ghi nhận thời điểm gieo trồng và tính thời gian tăng trưởng. Khi đủ thời gian, cây có thể thu hoạch và tiền được cộng vào tài khoản. Ô đất sau đó trở lại trạng thái trống để tiếp tục sử dụng.
Trong game có cửa hàng để mua hạt giống hoặc vật phẩm. Khi người chơi chọn mua, hệ thống kiểm tra số tiền hiện có. Nếu đủ tiền, tiền sẽ bị trừ và vật phẩm được thêm vào kho. Nếu không đủ, hệ thống hiển thị thông báo.
Mỗi hành động như trồng cây, thu hoạch, mua vật phẩm hay tăng level đều được lưu lại. Nhờ vậy, khi người chơi thoát game và đăng nhập lại sau này, toàn bộ tiến trình vẫn được giữ nguyên.

1. Trang giới thiệu game (Landing Page)

Trang chính của web.

Nội dung nên có:

Tên game

Trailer / video gameplay

Ảnh screenshot

Mô tả game

Giá game

Nút Mua game / Download

Có thể thêm:

yêu cầu cấu hình

roadmap

changelog update

2. Hệ thống tài khoản

Người chơi cần tài khoản để:

mua game

tải game

lưu lịch sử

Chức năng:

đăng ký

đăng nhập

quên mật khẩu

trang hồ sơ

danh sách game đã mua

Nhưng vì chỉ có 1 game, bảng dữ liệu cực đơn giản.

3. Hệ thống thanh toán (Sepay)

Flow sẽ như này:

User → bấm Mua Game
     ↓
Tạo Order
     ↓
Sepay tạo QR
     ↓
User chuyển tiền
     ↓
Sepay gửi webhook
     ↓
Server xác nhận thanh toán
     ↓
Mở quyền download
4. Hệ thống download

Sau khi mua:

User có thể:

tải game

tải bản update

Nên có:

link download

version game

changelog

5. Thống kê

Bạn muốn biết:

Thống kê truy cập

tổng lượt truy cập

người online

Thống kê bán hàng

bao nhiêu người mua

tổng doanh thu

Có thể dùng:

Google Analytics

hoặc dashboard riêng

6. Admin panel

Chỉ cần đơn giản:

Quản lý user

danh sách user

email

ngày đăng ký

Quản lý đơn hàng

ai đã mua

số tiền

thời gian