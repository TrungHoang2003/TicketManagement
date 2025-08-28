namespace BuildingBlocks.EmailHelper;

public static class EmailTemplates
{
    public static string GetTicketCreatedTemplate(string headDepartmentName, string ticketTitle, int ticketId, string creatorName, string priority)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
                .container {{ max-width: 600px; margin: 0 auto; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .ticket-info {{ border: 1px solid #dee2e6; padding: 15px; margin: 15px 0; }}
                .btn {{ background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                .priority-high {{ color: #dc3545; font-weight: bold; }}
                .priority-medium {{ color: #ffc107; font-weight: bold; }}
                .priority-low {{ color: #28a745; font-weight: bold; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🎫 Ticket Management System</h1>
                </div>
                <div class='content'>
                    <h2>Xin chào {headDepartmentName}!</h2>
                    <p>Có một ticket mới cần được xem xét trong hệ thống:</p>
                    
                    <div class='ticket-info'>
                        <h3>📋 Thông tin Ticket</h3>
                        <ul>
                            <li><strong>ID:</strong> #{ticketId}</li>
                            <li><strong>Tiêu đề:</strong> {ticketTitle}</li>
                            <li><strong>Người tạo:</strong> {creatorName}</li>
                            <li><strong>Độ ưu tiên:</strong> <span class='priority-{priority.ToLower()}'>{priority}</span></li>
                            <li><strong>Thời gian tạo:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                        </ul>
                    </div>
                    
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='http://localhost:5105/ticket/{ticketId}' class='btn'>
                            👀 Xem chi tiết Ticket
                        </a>
                    </p>
                    
                    <p><em>Vui lòng đăng nhập vào hệ thống để xem chi tiết và phân công xử lý ticket này.</em></p>
                </div>
                <div style='background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; color: #6c757d;'>
                    <p>Email này được gửi tự động từ Ticket Management System</p>
                </div>
            </div>
        </body>
        </html>";
    }
    
    public static string GetTicketAssignedTemplate(string assigneeName, string ticketTitle, int ticketId, string creatorName, string priority)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
                .container {{ max-width: 600px; margin: 0 auto; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .ticket-info {{ border: 1px solid #dee2e6; padding: 15px; margin: 15px 0; }}
                .btn {{ background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                .priority-high {{ color: #dc3545; font-weight: bold; }}
                .priority-medium {{ color: #ffc107; font-weight: bold; }}
                .priority-low {{ color: #28a745; font-weight: bold; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🎫 Ticket Management System</h1>
                </div>
                <div class='content'>
                    <h2>Xin chào {assigneeName}!</h2>
                    <p>Có một ticket mới cần được bạn xử lý trong hệ thống:</p>
                    
                    <div class='ticket-info'>
                        <h3>📋 Thông tin Ticket</h3>
                        <ul>
                            <li><strong>ID:</strong> #{ticketId}</li>
                            <li><strong>Tiêu đề:</strong> {ticketTitle}</li>
                            <li><strong>Người tạo:</strong> {creatorName}</li>
                            <li><strong>Độ ưu tiên:</strong> <span class='priority-{priority.ToLower()}'>{priority}</span></li>
                            <li><strong>Thời gian tạo:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                        </ul>
                    </div>
                    
                    <p style='text-align: center; margin: 30px 0;'>
                        <a href='http://localhost:5105/ticket/{ticketId}' class='btn'>
                            👀 Xem chi tiết Ticket
                        </a>
                    </p>
                    
                    <p><em>Vui lòng đăng nhập vào hệ thống để xem chi tiết và xử lý ticket này.</em></p>
                </div>
                <div style='background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; color: #6c757d;'>
                    <p>Email này được gửi tự động từ Ticket Management System</p>
                </div>
            </div>
        </body>
        </html>";
    }
    
    public static string GetTicketRejectedTemplate(string ticketTitle, int ticketId, string creatorName, string priority, string reason)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
                .container {{ max-width: 600px; margin: 0 auto; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .ticket-info {{ border: 1px solid #dee2e6; padding: 15px; margin: 15px 0; }}
                .btn {{ background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; }}
                .priority-high {{ color: #dc3545; font-weight: bold; }}
                .priority-medium {{ color: #ffc107; font-weight: bold; }}
                .priority-low {{ color: #28a745; font-weight: bold; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🎫 Ticket Management System</h1>
                </div>
                <div class='content'>
                    <h2>Xin chào {creatorName}!</h2>
                    <p>Ticket của bạn đã bị từ chối với lý do: {reason}</p>
                    
                    <div class='ticket-info'>
                        <h3>📋 Thông tin Ticket</h3>
                        <ul>
                            <li><strong>ID:</strong> #{ticketId}</li>
                            <li><strong>Tiêu đề:</strong> {ticketTitle}</li>
                            <li><strong>Người tạo:</strong> {creatorName}</li>
                            <li><strong>Độ ưu tiên:</strong> <span class='priority-{priority.ToLower()}'>{priority}</span></li>
                            <li><strong>Thời gian tạo:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                        </ul>
                    </div>
                    
                </div>
                <div style='background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; color: #6c757d;'>
                    <p>Email này được gửi tự động từ Ticket Management System</p>
                </div>
            </div>
        </body>
        </html>";
    }
}