namespace CrackTheCode.Domain.Enums
{
    public enum ClueType
    {
        CorrectDigitsAndPositions,        // X correct digits and in correct positions (Bulls)
        CorrectDigitsWrongPositions,      // X correct digits but in wrong positions (Cows)
        AllWrong,                         // No digits correct (0 correct digits)
        SumEquals,                        // Sum of digits = X
        SumGreaterThan,                   // Sum of digits > X
        SumLessThan,                      // Sum of digits < X
        HasEven,                          // Has even digits (at least one)
        HasOdd,                           // Has odd digits (at least one)
        ExactlyXPrimeDigits,              // Exactly X prime digits
        ExactlyXDigitsGreaterThanFive,    // Exactly X digits > 5
        AtLeastOneRepeatingDigit,         // At least one repeating digit
        MaxDigitEquals,                   // Max digit is X
        MinDigitEquals,                   // Min digit is X
        HasConsecutiveDigits,             // Has at least two consecutive digits (e.g. difference is 1)
        IsPalindrome,                     // Code is a palindrome (reads the same forwards and backwards)
        SumPositionsOneAndTwoEquals,      // Sum of digit at index 0 and index 1 = X
        FirstDigitGreaterThanLast,        // First digit > last digit
        MiddleDigitIsPrime                // Middle digit is prime (for odd lengths, or second digit for even lengths)
    }
}