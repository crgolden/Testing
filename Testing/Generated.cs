namespace Testing;

using System.Globalization;

public static class Generated
{
    private const int SmallestBlankLength = 1;
    private const int LargestBlankLength = 4;
    private const int LargestPrice = 1000;
    private const int PriceDecimalPlaces = 2;
    private const int ProductNameFirstWordLength = 5;
    private const int ProductNameSecondWordLength = 7;
    private const int BrandLength = 6;
    private const int ModelNumberPrefixLength = 3;
    private const int ModelNumberSuffixLength = 5;
    private const int CategoryLength = 8;
    private const int HostLength = 12;
    private const int PathLength = 6;
    private const int SmallestMinutesAgo = 1;
    private const int LargestMinutesAgo = 100000;
    private const int SmallestOffsetHours = 1;
    private const int LargestOffsetHours = 13;
    private const int SmallestZip = 10000;
    private const int LargestZip = 100000;
    private const int CityLength = 9;
    private const int SmallestStreetNumber = 100;
    private const int LargestStreetNumber = 10000;
    private const int StreetNameLength = 10;
    private const int DescriptionFirstWordLength = 7;
    private const int DescriptionSecondWordLength = 9;
    private const int DescriptionThirdWordLength = 5;
    private const int SmallestAreaCode = 200;
    private const int LargestAreaCode = 1000;
    private const int SmallestLineNumber = 1000;
    private const int LargestLineNumber = 10000;
    private const int EmailLocalPartLength = 10;
    private const int EmailDomainLength = 8;
    private const string EveryPasswordCharacterClass = "Aa1!";
    private const int SmallestPasswordTailLength = 6;
    private const int LargestPasswordTailLength = 12;
    private const int DaysInWeek = 7;
    private const int HostLabelLength = 8;
    private const char FirstLetter = 'a';
    private const char FirstLetterOfSecondHalf = 'n';
    private const char LastLetter = 'z';
    private const int NameFirstWordLength = 6;
    private const int NameSecondWordLength = 8;
    private const int SlugFirstWordLength = 6;
    private const int SlugSecondWordLength = 8;
    private const int LanguageLength = 7;
    private const int WebsiteHostLength = 12;
    private const int FailureTokenLength = 10;
    private const int SmallestUnparseableStateCodeLength = 3;
    private const int LargestUnparseableStateCodeLength = 10;
    private const double SmallestLatitude = -90.0;
    private const double LatitudeSpan = 180.0;
    private const double SmallestLongitude = -180.0;
    private const double LongitudeSpan = 360.0;
    private const int CoordinateDecimalPlaces = 6;
    private const int HoursInDay = 24;
    private const int MinutesInHour = 60;

    public static string LowercaseToken(int length) =>
        string.Concat(Enumerable.Range(0, length).Select(_ => (char)Random.Shared.Next(FirstLetter, LastLetter + 1)));

    public static string NewBlank() =>
        new string(' ', Random.Shared.Next(SmallestBlankLength, LargestBlankLength));

    public static string NewProductName() =>
        $"{LowercaseToken(ProductNameFirstWordLength)} {LowercaseToken(ProductNameSecondWordLength)}";

    public static string NewBrand() => LowercaseToken(BrandLength);

    public static string NewModelNumber() =>
        $"{LowercaseToken(ModelNumberPrefixLength)}-{LowercaseToken(ModelNumberSuffixLength)}";

    public static string NewCategory() => LowercaseToken(CategoryLength);

    public static decimal NewPrice() =>
        Math.Round((decimal)(Random.Shared.NextDouble() * LargestPrice), PriceDecimalPlaces);

    public static Uri NewManualUrl() =>
        new Uri($"https://{LowercaseToken(HostLength)}.example/{LowercaseToken(PathLength)}");

    public static Guid NewUserId() => Guid.NewGuid();

    public static DateTimeOffset NewUtcTimestamp() =>
        DateTimeOffset.UtcNow.AddMinutes(-Random.Shared.Next(SmallestMinutesAgo, LargestMinutesAgo));

    public static DateTimeOffset NewTimestampWithNonZeroOffset() =>
        NewUtcTimestamp().ToOffset(TimeSpan.FromHours(-Random.Shared.Next(SmallestOffsetHours, LargestOffsetHours)));

    public static string NewEmailAddress() =>
        $"{LowercaseToken(EmailLocalPartLength)}@{LowercaseToken(EmailDomainLength)}.invalid";

    public static string NewPassword() =>
        EveryPasswordCharacterClass
        + LowercaseToken(Random.Shared.Next(SmallestPasswordTailLength, LargestPasswordTailLength));

    public static string NewPhoneNumber() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{Random.Shared.Next(SmallestAreaCode, LargestAreaCode)}-{Random.Shared.Next(SmallestAreaCode, LargestAreaCode)}-{Random.Shared.Next(SmallestLineNumber, LargestLineNumber)}");

    public static string NewCity() => LowercaseToken(CityLength);

    public static string NewStreet() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{Random.Shared.Next(SmallestStreetNumber, LargestStreetNumber)} {LowercaseToken(StreetNameLength)} street");

    public static string NewZip() =>
        Random.Shared.Next(SmallestZip, LargestZip).ToString(CultureInfo.InvariantCulture);

    public static string NewDescription() =>
        $"{LowercaseToken(DescriptionFirstWordLength)} {LowercaseToken(DescriptionSecondWordLength)} {LowercaseToken(DescriptionThirdWordLength)}";

    public static byte NewDayOfWeek() => (byte)Random.Shared.Next(0, DaysInWeek);

    public static string NewHostLabel() => LowercaseToken(HostLabelLength);

    public static string NewTokenFromFirstHalfOfAlphabet(int length) =>
        string.Concat(
            Enumerable.Range(0, length).Select(_ => (char)Random.Shared.Next(FirstLetter, FirstLetterOfSecondHalf)));

    public static string NewTokenFromSecondHalfOfAlphabet(int length) =>
        string.Concat(
            Enumerable.Range(0, length).Select(_ => (char)Random.Shared.Next(FirstLetterOfSecondHalf, LastLetter + 1)));

    public static string NewName() => $"{LowercaseToken(NameFirstWordLength)} {LowercaseToken(NameSecondWordLength)}";

    public static string NewSlug() => $"{LowercaseToken(SlugFirstWordLength)}-{LowercaseToken(SlugSecondWordLength)}";

    public static string NewLanguage() => LowercaseToken(LanguageLength);

    public static string NewWebsite() => $"https://{LowercaseToken(WebsiteHostLength)}.example";

    public static string NewFailureMessage() => $"failure-{LowercaseToken(FailureTokenLength)}";

    public static string NewFailureReason() => NewFailureMessage();

    public static string NewUnparseableStateCode() =>
        LowercaseToken(Random.Shared.Next(SmallestUnparseableStateCodeLength, LargestUnparseableStateCodeLength));

    public static double NewLatitude() =>
        Math.Round((Random.Shared.NextDouble() * LatitudeSpan) + SmallestLatitude, CoordinateDecimalPlaces);

    public static double NewLongitude() =>
        Math.Round((Random.Shared.NextDouble() * LongitudeSpan) + SmallestLongitude, CoordinateDecimalPlaces);

    public static TimeOnly NewTimeOfDay() =>
        new TimeOnly(Random.Shared.Next(0, HoursInDay), Random.Shared.Next(0, MinutesInHour));

    public static decimal NewRoundedFraction(int decimalPlaces) =>
        Math.Round((decimal)Random.Shared.NextDouble(), decimalPlaces);

    public static Guid NewChurchId() => Guid.NewGuid();
}
