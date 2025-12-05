# Admin Dashboard - Ticket Management System

## 📊 Tổng quan

Admin Dashboard là một tính năng mới được thêm vào hệ thống Ticket Management, cho phép Admin có cái nhìn tổng quan về hiệu suất toàn bộ hệ thống, phòng ban và nhân viên.

## ✨ Tính năng chính

### 1. **System Overview (Tổng quan hệ thống)**
- Tổng số Users, Active Users
- Tổng số Phòng ban
- Tổng số Tickets theo từng trạng thái (Pending, In Progress, Completed, Rejected)
- Tỷ lệ hoàn thành tổng thể
- Tỷ lệ hoàn thành đúng hạn
- Thời gian xử lý trung bình

### 2. **Employee Performance (Hiệu suất nhân viên)**
- **Top Performers**: Top 10 nhân viên có hiệu suất tốt nhất
  - Số tickets được giao và hoàn thành
  - Tỷ lệ hoàn thành (%)
  - Tỷ lệ đúng hạn (%)
  - Thời gian xử lý trung bình
  - Đánh giá hiệu suất: Excellent, Good, Average, Poor
  
### 3. **Department Performance (Hiệu suất phòng ban)**
- So sánh hiệu suất giữa các phòng ban
- Số lượng nhân viên và nhân viên hoạt động
- Tickets theo từng trạng thái
- Tỷ lệ hoàn thành và đúng hạn
- Phân bổ khối lượng công việc
- Top 3 categories được xử lý nhiều nhất

### 4. **Trend Analysis (Phân tích xu hướng)**
- Biểu đồ xu hướng tickets theo thời gian (6 tháng gần nhất)
- Số lượng tickets: Tạo mới, Hoàn thành, Đang xử lý
- Thời gian xử lý trung bình theo tháng
- Số lượng users hoạt động theo tháng

### 5. **Real-time Alerts (Cảnh báo thời gian thực)**
- **SLA Violations**: Tickets quá hạn
- **High Workload**: Nhân viên có khối lượng công việc cao
- **Long Pending**: Tickets chờ xử lý quá lâu
- Phân loại theo mức độ: Critical, Warning, Info

### 6. **Workload Analysis (Phân tích khối lượng công việc)**
- Phân tích chi tiết khối lượng công việc của từng nhân viên
- Số lượng tickets hiện tại, ưu tiên cao, quá hạn
- Điểm khối lượng công việc (Workload Score)
- Mức độ: Light, Normal, Heavy, Overloaded

## 🚀 Cách sử dụng

### 1. Truy cập Dashboard
- Đăng nhập với tài khoản **Admin**
- Vào menu **Admin** → **Dashboard Admin**

### 2. Điều hướng giữa các Tab
Dashboard có 5 tabs chính:
- **Tổng quan**: Thống kê tổng thể hệ thống
- **Hiệu suất**: Top performers và hiệu suất phòng ban
- **Xu hướng**: Biểu đồ và phân tích theo thời gian
- **Cảnh báo**: Các vấn đề cần xử lý ngay
- **Khối lượng công việc**: Phân tích workload của nhân viên

### 3. Phân tích và Ra quyết định
- Xác định top performers để khen thưởng
- Phát hiện nhân viên cần hỗ trợ (underperformers)
- So sánh hiệu suất giữa các phòng ban
- Phân bổ lại công việc khi phát hiện workload cao
- Xử lý tickets quá hạn và cảnh báo kịp thời

## 🔧 API Endpoints

### Backend APIs
```
GET /admin/dashboard - Lấy toàn bộ dữ liệu dashboard
GET /admin/dashboard/system-overview - Tổng quan hệ thống
GET /admin/dashboard/employee-performance?departmentId={id}&topCount={count} - Hiệu suất nhân viên
GET /admin/dashboard/department-performance - Hiệu suất phòng ban
GET /admin/dashboard/trend-analysis?months={months} - Phân tích xu hướng
GET /admin/dashboard/alerts - Cảnh báo thời gian thực
GET /admin/dashboard/workload-analysis - Phân tích khối lượng công việc
```

### Authorization
- Tất cả endpoints yêu cầu **Admin role**
- Sử dụng JWT Token authentication
- Policy: `AdminOnly`

## 📁 Cấu trúc Code

### Backend (C# .NET Core)
```
Application/
├── DTOs/
│   └── AdminDashboardDto.cs - Tất cả DTOs cho dashboard
├── Services/
│   └── AdminDashboardService.cs - Business logic
└── ApplicationDi.cs - Dependency Injection

TicketManagement.Api/
└── Controllers/
    └── AdminDashboardController.cs - API endpoints
```

### Frontend (React TypeScript)
```
src/
├── api/
│   ├── adminDashboardApi.ts - API functions
│   └── index.ts - Export tất cả APIs
├── pages/
│   └── admin/
│       └── AdminDashboard.tsx - Dashboard component
└── shared/
    └── components/
        └── admin/
            └── AdminSidebar.tsx - Navigation menu
```

## 📊 Metrics và KPIs

### Performance Metrics
- **Completion Rate**: Tỷ lệ tickets hoàn thành / tổng tickets
- **On-Time Rate**: Tỷ lệ tickets hoàn thành đúng hạn
- **Average Resolution Days**: Thời gian xử lý trung bình
- **Workload Score**: Điểm khối lượng công việc (weighted)

### Performance Ratings
**Employee Performance:**
- **Excellent**: Score ≥ 150
- **Good**: Score ≥ 120
- **Average**: Score ≥ 80
- **Poor**: Score < 80

**Department Performance:**
- **Excellent**: Score ≥ 140
- **Good**: Score ≥ 100
- **Average**: Score ≥ 70
- **Poor**: Score < 70

## 🎯 Use Cases

### 1. Đánh giá hiệu suất định kỳ
- Xem top performers hàng tháng
- So sánh hiệu suất giữa các phòng ban
- Xác định xu hướng tăng/giảm

### 2. Phân bổ nguồn lực
- Phát hiện phòng ban/nhân viên quá tải
- Cân bằng workload giữa các nhân viên
- Quyết định tuyển dụng thêm nhân sự

### 3. Cải thiện quy trình
- Phân tích thời gian xử lý trung bình
- Xác định loại tickets phức tạp
- Tối ưu hóa workflow

### 4. Quản lý SLA
- Theo dõi tickets quá hạn
- Cảnh báo sớm về các vấn đề
- Đảm bảo chất lượng dịch vụ

## 🔄 Cập nhật dữ liệu

Dashboard tự động fetch dữ liệu khi:
- Lần đầu load trang
- Chuyển đổi giữa các tabs
- Click nút "Thử lại" khi có lỗi

**Lưu ý**: Dữ liệu được cache ở frontend để tránh load lại không cần thiết.

## 🎨 UI/UX Features

### Responsive Design
- Desktop: 7xl container với full features
- Tablet: Grid layout tự động điều chỉnh
- Mobile: Single column layout

### Visual Elements
- **Charts**: Line, Bar, Pie, Area charts (Recharts)
- **Tables**: Sortable, với highlight cho top performers
- **Cards**: Statistics cards với icons
- **Badges**: Performance ratings, alert severity
- **Colors**: Semantic colors (green=good, red=bad, yellow=warning)

### Interactive Features
- Tab navigation
- Hover effects
- Loading states
- Error handling với retry button
- Real-time update indicator

## 🛠️ Troubleshooting

### Lỗi thường gặp

**1. "Unauthorized" khi truy cập**
- Kiểm tra đã đăng nhập với role Admin
- Verify JWT token còn hiệu lực

**2. "Failed to load dashboard data"**
- Kiểm tra backend server đang chạy
- Verify API endpoints accessible
- Check console logs

**3. Dữ liệu không chính xác**
- Clear browser cache
- Refresh lại trang
- Kiểm tra database data

## 📝 Changelog

### Version 1.0.0 (2024-12-05)
- ✨ Initial release
- ✅ System Overview
- ✅ Employee Performance tracking
- ✅ Department Performance comparison
- ✅ Trend Analysis (6 months)
- ✅ Real-time Alerts
- ✅ Workload Analysis
- ✅ Full responsive design
- ✅ Admin-only authorization

## 🔮 Future Enhancements

- [ ] Export dashboard to PDF/Excel
- [ ] Email reports tự động
- [ ] Custom date range cho trend analysis
- [ ] Advanced filtering và sorting
- [ ] Dashboard customization (drag & drop widgets)
- [ ] Performance comparison between time periods
- [ ] Predictive analytics với AI/ML
- [ ] Real-time notifications
- [ ] Mobile app

## 👥 Credits

Developed by: [Your Team Name]
Date: December 2024
Version: 1.0.0