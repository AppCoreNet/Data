// Licensed under the MIT License.
// Copyright (c) 2012 Markus Ullmark

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#nullable disable

namespace AppCore.Data;

/// <summary>
/// Generate YouTube-like hashes from one or many numbers. Use hashids when you do not want to expose your database ids to the user.
/// </summary>
internal class Hashids
{
    public const string DefaultAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    public const string DefaultSeps = "cfhistuCFHISTU";

    private const int MinAlphabetLength = 16;
    private const double SepDiv = 3.5;
    private const double GuardDiv = 12.0;

    private string _alphabet;
    private string _salt;
    private string _seps;
    private string _guards;
    private int _minHashLength;

    private Regex _guardsRegex;
    private Regex _sepsRegex;

    //  Creates the Regex in the first usage, speed up first use of non hex methods
#if NETSTANDARD1_2
        private static Lazy<Regex> hexValidator = new Lazy<Regex>(() => new Regex("^[0-9a-fA-F]+$"));
        private static Lazy<Regex> hexSplitter = new Lazy<Regex>(() => new Regex(@"[\w\W]{1,12}"));
#else
    private static Lazy<Regex> _hexValidator = new Lazy<Regex>(() => new Regex("^[0-9a-fA-F]+$", RegexOptions.Compiled));
    private static Lazy<Regex> _hexSplitter = new Lazy<Regex>(() => new Regex(@"[\w\W]{1,12}", RegexOptions.Compiled));
#endif

    /// <summary>
    /// Instantiates a new Hashids with the default setup.
    /// </summary>
    public Hashids() : this(string.Empty, 0, DefaultAlphabet, DefaultSeps)
    {}

    /// <summary>
    /// Instantiates a new Hashids en/de-coder.
    /// </summary>
    /// <param name="salt"></param>
    /// <param name="minHashLength"></param>
    /// <param name="alphabet"></param>
    /// <param name="seps"></param>
    public Hashids(string salt = "", int minHashLength = 0, string alphabet = DefaultAlphabet, string seps = DefaultSeps)
    {
        if (string.IsNullOrWhiteSpace(alphabet))
            throw new ArgumentNullException("alphabet");

        this._salt = salt;
        this._alphabet = new string(alphabet.ToCharArray().Distinct().ToArray());
        this._seps = seps;
        this._minHashLength = minHashLength;

        if (this._alphabet.Length < 16)
            throw new ArgumentException("alphabet must contain atleast 4 unique characters.", "alphabet");

        this.SetupSeps();
        this.SetupGuards();
    }

    /// <summary>
    /// Encodes the provided numbers into a hashed string
    /// </summary>
    /// <param name="numbers">the numbers to encode</param>
    /// <returns>the hashed string</returns>
    public virtual string Encode(params int[] numbers)
    {
        if (numbers.Any(n => n < 0)) return string.Empty;
        return this.GenerateHashFrom(numbers.Select(n => (long)n).ToArray());
    }

    /// <summary>
    /// Encodes the provided numbers into a hashed string
    /// </summary>
    /// <param name="numbers">the numbers to encode</param>
    /// <returns>the hashed string</returns>
    public virtual string Encode(IEnumerable<int> numbers)
    {
        return this.Encode(numbers.ToArray());
    }

    /// <summary>
    /// Decodes the provided hash into
    /// </summary>
    /// <param name="hash">the hash</param>
    /// <exception cref="T:System.OverflowException">if the decoded number overflows integer</exception>
    /// <returns>the numbers</returns>
    public virtual int[] Decode(string hash)
    {
        return this.GetNumbersFrom(hash).Select(n => (int)n).ToArray();
    }

    /// <summary>
    /// Encodes the provided hex string to a hashids hash.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public virtual string EncodeHex(string hex)
    {
        if (!_hexValidator.Value.IsMatch(hex))
            return string.Empty;

        var numbers = new List<long>();
        MatchCollection matches = _hexSplitter.Value.Matches(hex);

        foreach (Match match in matches)
        {
            long number = Convert.ToInt64(string.Concat("1", match.Value), 16);
            numbers.Add(number);
        }

        return this.EncodeLong(numbers.ToArray());
    }

    /// <summary>
    /// Decodes the provided hash into a hex-string
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public virtual string DecodeHex(string hash)
    {
        var ret = new StringBuilder();
        long[] numbers = this.DecodeLong(hash);

        foreach (long number in numbers)
            ret.Append(string.Format("{0:X}", number).Substring(1));

        return ret.ToString();
    }

    /// <summary>
    /// Decodes the provided hashed string into an array of longs
    /// </summary>
    /// <param name="hash">the hashed string</param>
    /// <returns>the numbers</returns>
    public long[] DecodeLong(string hash)
    {
        return this.GetNumbersFrom(hash);
    }

    /// <summary>
    /// Encodes the provided longs to a hashed string
    /// </summary>
    /// <param name="numbers">the numbers</param>
    /// <returns>the hashed string</returns>
    public string EncodeLong(params long[] numbers)
    {
        if (numbers.Any(n => n < 0)) return string.Empty;
        return this.GenerateHashFrom(numbers);
    }

    /// <summary>
    /// Encodes the provided longs to a hashed string
    /// </summary>
    /// <param name="numbers">the numbers</param>
    /// <returns>the hashed string</returns>
    public string EncodeLong(IEnumerable<long> numbers)
    {
        return this.EncodeLong(numbers.ToArray());
    }

    /// <summary>
    /// Encodes the provided numbers into a string.
    /// </summary>
    /// <param name="numbers">the numbers</param>
    /// <returns>the hash</returns>
    [Obsolete("Use 'Encode' instead. The method was renamed to better explain what it actually does.")]
    public virtual string Encrypt(params int[] numbers)
    {
        return Encode(numbers);
    }

    /// <summary>
    /// Encrypts the provided hex string to a hashids hash.
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    [Obsolete("Use 'EncodeHex' instead. The method was renamed to better explain what it actually does.")]
    public virtual string EncryptHex(string hex)
    {
        return EncodeHex(hex);
    }

    /// <summary>
    /// Decodes the provided numbers into a array of numbers
    /// </summary>
    /// <param name="hash">hash</param>
    /// <returns>array of numbers.</returns>
    [Obsolete("Use 'Decode' instead. Method was renamed to better explain what it actually does.")]
    public virtual int[] Decrypt(string hash)
    {
        return Decode(hash);
    }

    /// <summary>
    /// Decodes the provided hash to a hex-string
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    [Obsolete("Use 'DecodeHex' instead. The method was renamed to better explain what it actually does.")]
    public virtual string DecryptHex(string hash)
    {
        return DecodeHex(hash);
    }

    /// <summary>
    ///
    /// </summary>
    private void SetupSeps()
    {
        // seps should contain only characters present in alphabet;
        _seps = new string(_seps.ToCharArray().Intersect(_alphabet.ToCharArray()).ToArray());

        // alphabet should not contain seps.
        _alphabet = new string(_alphabet.ToCharArray().Except(_seps.ToCharArray()).ToArray());

        _seps = ConsistentShuffle(_seps, _salt);

        if (_seps.Length == 0 || _alphabet.Length / _seps.Length > SepDiv)
        {
            int sepsLength = (int)Math.Ceiling(_alphabet.Length / SepDiv);
            if (sepsLength == 1)
                sepsLength = 2;

            if (sepsLength > _seps.Length)
            {
                int diff = sepsLength - _seps.Length;
                _seps += _alphabet.Substring(0, diff);
                _alphabet = _alphabet.Substring(diff);
            }

            else _seps = _seps.Substring(0, sepsLength);
        }

#if NETSTANDARD1_2
            sepsRegex = new Regex(string.Concat("[", seps, "]"));
#else
        _sepsRegex = new Regex(string.Concat("[", _seps, "]"), RegexOptions.Compiled);
#endif
        _alphabet = ConsistentShuffle(_alphabet, _salt);
    }

    /// <summary>
    ///
    /// </summary>
    private void SetupGuards()
    {
        int guardCount = (int)Math.Ceiling(_alphabet.Length / GuardDiv);

        if (_alphabet.Length < 3)
        {
            _guards = _seps.Substring(0, guardCount);
            _seps = _seps.Substring(guardCount);
        }

        else
        {
            _guards = _alphabet.Substring(0, guardCount);
            _alphabet = _alphabet.Substring(guardCount);
        }

#if NETSTANDARD1_2
            guardsRegex = new Regex(string.Concat("[", guards, "]"));
#else
        _guardsRegex = new Regex(string.Concat("[", _guards, "]"), RegexOptions.Compiled);
#endif
    }

    /// <summary>
    /// Internal function that does the work of creating the hash
    /// </summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
    private string GenerateHashFrom(long[] numbers)
    {
        if (numbers == null || numbers.Length == 0)
            return string.Empty;

        var ret = new StringBuilder();
        string alphabet = this._alphabet;

        long numbersHashInt = 0;
        for (int i = 0; i < numbers.Length; i++)
            numbersHashInt += (int)(numbers[i] % (i + 100));

        char lottery = alphabet[(int)(numbersHashInt % alphabet.Length)];
        ret.Append(lottery.ToString());

        for (int i = 0; i < numbers.Length; i++)
        {
            long number = numbers[i];
            string buffer = lottery + this._salt + alphabet;

            alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
            string last = this.Hash(number, alphabet);

            ret.Append(last);

            if (i + 1 < numbers.Length)
            {
                number %= ((int)last[0] + i);
                int sepsIndex = ((int)number % this._seps.Length);

                ret.Append(this._seps[sepsIndex]);
            }
        }

        if (ret.Length < this._minHashLength)
        {
            int guardIndex = ((int)(numbersHashInt + (int)ret[0]) % this._guards.Length);
            char guard = this._guards[guardIndex];

            ret.Insert(0, guard);

            if (ret.Length < this._minHashLength)
            {
                guardIndex = ((int)(numbersHashInt + (int)ret[2]) % this._guards.Length);
                guard = this._guards[guardIndex];

                ret.Append(guard);
            }
        }

        int halfLength = (int)(alphabet.Length / 2);
        while (ret.Length < this._minHashLength)
        {
            alphabet = ConsistentShuffle(alphabet, alphabet);
            ret.Insert(0, alphabet.Substring(halfLength));
            ret.Append(alphabet.Substring(0, halfLength));

            int excess = ret.Length - this._minHashLength;
            if (excess > 0)
            {
                ret.Remove(0, excess / 2);
                ret.Remove(this._minHashLength, ret.Length - this._minHashLength);
            }
        }

        return ret.ToString();
    }

    private string Hash(long input, string alphabet)
    {
        var hash = new StringBuilder();

        do
        {
            hash.Insert(0, alphabet[(int)(input % alphabet.Length)]);
            input = (input / alphabet.Length);
        } while (input > 0);

        return hash.ToString();
    }

    private long Unhash(string input, string alphabet)
    {
        long number = 0;

        for (int i = 0; i < input.Length; i++)
        {
            int pos = alphabet.IndexOf(input[i]);
            number += (long)(pos * Math.Pow(alphabet.Length, input.Length - i - 1));
        }

        return number;
    }

    private long[] GetNumbersFrom(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return new long[0];

        string alphabet = new string(this._alphabet.ToCharArray());
        var ret = new List<long>();
        int i = 0;

        string hashBreakdown = _guardsRegex.Replace(hash, " ");
        string[] hashArray = hashBreakdown.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (hashArray.Length == 3 || hashArray.Length == 2)
            i = 1;

        hashBreakdown = hashArray[i];
        if (hashBreakdown[0] != default(char))
        {
            char lottery = hashBreakdown[0];
            hashBreakdown = hashBreakdown.Substring(1);

            hashBreakdown = _sepsRegex.Replace(hashBreakdown, " ");
            hashArray = hashBreakdown.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int j = 0; j < hashArray.Length; j++)
            {
                string subHash = hashArray[j];
                string buffer = lottery + this._salt + alphabet;

                alphabet = ConsistentShuffle(alphabet, buffer.Substring(0, alphabet.Length));
                ret.Add(Unhash(subHash, alphabet));
            }

            if (EncodeLong(ret.ToArray()) != hash)
                ret.Clear();
        }

        return ret.ToArray();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="alphabet"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    private string ConsistentShuffle(string alphabet, string salt)
    {
        if (string.IsNullOrWhiteSpace(salt))
            return alphabet;

        int n;
        char[] letters = alphabet.ToCharArray();
        for (int i = letters.Length - 1, v = 0, p = 0; i > 0; i--, v++)
        {
            v %= salt.Length;
            p += (n = salt[v]);
            int j = (n + v + p) % i;
            // swap characters at positions i and j
            char temp = letters[j];
            letters[j] = letters[i];
            letters[i] = temp;
        }

        return new string(letters);
    }

}