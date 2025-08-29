using BuildingBlocks.Commons;
using Domain.Entities;

namespace Application.Mappings;

public static class EnumMapping
{
    public static Status ToStatusEnum(this string status)
    {
        var lowerStatus = status.ToLowerInvariant();
        
        return lowerStatus switch
        {
            "pending" => Status.Pending,
            "received" => Status.Received,
            "inprogress" => Status.InProgress,
            "rejected" => Status.Rejected,
            "closed" => Status.Closed,
            _ => throw new BusinessException("This status is a not valid status")
        };
    }
}