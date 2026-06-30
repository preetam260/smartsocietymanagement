namespace SmartSociety.Domain.Models;

public class Apartment : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Block {get; set;} = "";
    public int Floor {get; set;}
    public string Number {get; set;} = "";

    public User? Owner { get; set; }
    public ICollection<Resident> Residents {get; set;} = new List<Resident>();
    public ICollection<Bill> Bills {get; set;} = new List<Bill>();
    public ICollection<Visitor> Visitors {get; set;} = new List<Visitor>();
    
}