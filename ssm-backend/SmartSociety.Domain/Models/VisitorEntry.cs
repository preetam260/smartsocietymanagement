namespace SmartSociety.Domain.Models;

public class VisitorEntry : BaseEntity
{
    public Guid VisitorId {get; set;} 
    public DateTime CheckinTime {get; set;}
    public DateTime? CheckoutTime {get; set;}
    public Guid StaffId {get; set;} 

    public Visitor? Visitor {get; set;}
    public User? Staff {get; set;}

}