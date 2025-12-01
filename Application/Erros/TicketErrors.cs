using BuildingBlocks.Commons;

namespace Application.Erros;

public sealed class TicketErrors
{
   public static readonly Error TicketNotFound = new Error("Lỗi Ticket", "Không tìm thấy ticket");
   public static readonly Error AssigneesAlreadyAssigned = new Error("Lỗi Phân Công", "Tất cả nhân viên đã được phân công cho ticket này");
}