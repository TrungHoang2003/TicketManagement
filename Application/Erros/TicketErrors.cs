using BuildingBlocks.Commons;

namespace Application.Erros;

public sealed class TicketErrors
{
   public static readonly Error TicketNotFound = new Error("Lỗi Ticket", "KHông tìm thấy ticket");
}