using System.Text.Json;

namespace MedicalSupplies.Web.Services;

/// <summary>
/// A lightweight "request a quote" cart kept in the visitor's session —
/// ProductId -> Quantity. No sign-in required to build a quote list;
/// an account is only needed (indirectly, via email/phone) when they
/// actually submit the request.
/// </summary>
public interface IQuotationCartService
{
    Dictionary<int, int> GetCart(ISession session);
    void AddOrUpdate(ISession session, int productId, int quantity);
    void Remove(ISession session, int productId);
    void Clear(ISession session);
}

public class QuotationCartService : IQuotationCartService
{
    private const string SessionKey = "QuotationCart";

    public Dictionary<int, int> GetCart(ISession session)
    {
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return new Dictionary<int, int>();
        return JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
    }

    public void AddOrUpdate(ISession session, int productId, int quantity)
    {
        var cart = GetCart(session);
        if (quantity <= 0)
        {
            cart.Remove(productId);
        }
        else
        {
            cart[productId] = quantity;
        }
        Save(session, cart);
    }

    public void Remove(ISession session, int productId)
    {
        var cart = GetCart(session);
        cart.Remove(productId);
        Save(session, cart);
    }

    public void Clear(ISession session) => session.Remove(SessionKey);

    private static void Save(ISession session, Dictionary<int, int> cart) =>
        session.SetString(SessionKey, JsonSerializer.Serialize(cart));
}
