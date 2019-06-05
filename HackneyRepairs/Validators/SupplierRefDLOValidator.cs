using System.Text.RegularExpressions;
namespace HackneyRepairs.Validators
{
    public class SupplierRefDLOValidator
    {
        public static bool Validate(string supplierRef)
        {
            if (string.IsNullOrWhiteSpace(supplierRef))
                return false;
            return Regex.IsMatch(supplierRef, "^H[01][1-7]|10$", RegexOptions.IgnoreCase);
        }
    }
}
