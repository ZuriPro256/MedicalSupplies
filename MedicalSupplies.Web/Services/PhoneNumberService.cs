using System.Globalization;
using PhoneNumbers;

namespace MedicalSupplies.Web.Services;

public interface IPhoneNumberService
{
    bool TryNormalize(
        string? nationalNumber,
        string? countryCode,
        out string? e164Number,
        out string errorMessage);

    string? GetRegionCode(string? e164Number);

    IReadOnlyList<PhoneCountryOption> GetCountries();
}

public sealed class PhoneNumberService : IPhoneNumberService
{
    private readonly PhoneNumberUtil _phoneUtil;

    public PhoneNumberService()
    {
        _phoneUtil = PhoneNumberUtil.GetInstance();
    }

    public bool TryNormalize(
        string? nationalNumber,
        string? countryCode,
        out string? e164Number,
        out string errorMessage)
    {
        e164Number = null;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(nationalNumber))
        {
            errorMessage = "Phone number is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(countryCode))
        {
            errorMessage = "Please select a country.";
            return false;
        }

        countryCode = countryCode.Trim().ToUpperInvariant();

        if (!_phoneUtil.GetSupportedRegions().Contains(countryCode))
        {
            errorMessage = "The selected country is not supported.";
            return false;
        }

        try
        {
            var cleanedNumber = nationalNumber.Trim();

            if (cleanedNumber.StartsWith("+"))
            {
                errorMessage =
                    "Enter the phone number without the country code. Select the country above instead.";
                return false;
            }

            var parsedNumber = _phoneUtil.Parse(
                cleanedNumber,
                countryCode);

            if (!_phoneUtil.IsValidNumberForRegion(
                    parsedNumber,
                    countryCode))
            {
                errorMessage =
                    "Enter a valid phone number for the selected country.";
                return false;
            }

            e164Number = _phoneUtil.Format(
                parsedNumber,
                PhoneNumberFormat.E164);

            return true;
        }
        catch (NumberParseException)
        {
            errorMessage =
                "Enter a valid phone number for the selected country.";

            return false;
        }
    }

    public string? GetRegionCode(string? e164Number)
    {
        if (string.IsNullOrWhiteSpace(e164Number))
            return null;

        try
        {
            var parsedNumber = _phoneUtil.Parse(
                e164Number,
                null);

            return _phoneUtil
                .GetRegionCodeForNumber(parsedNumber)?
                .ToUpperInvariant();
        }
        catch (NumberParseException)
        {
            return null;
        }
    }

    public IReadOnlyList<PhoneCountryOption> GetCountries()
    {
        return _phoneUtil
            .GetSupportedRegions()
            .Select(regionCode =>
            {
                var countryCode =
                    _phoneUtil.GetCountryCodeForRegion(regionCode);

                string countryName;

                try
                {
                    countryName =
                        new RegionInfo(regionCode).EnglishName;
                }
                catch
                {
                    countryName = regionCode;
                }

                return new PhoneCountryOption
                {
                    Code = regionCode,
                    Name = countryName,
                    CallingCode = countryCode
                };
            })
            .OrderBy(c => c.Name)
            .ToList();
    }
}

public sealed class PhoneCountryOption
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int CallingCode { get; init; }
}
