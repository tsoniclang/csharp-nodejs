using System;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The URL class represents a parsed URL and provides properties for accessing and modifying URL components.
/// </summary>
public class URL
{
    private Uri _uri;
    private URLSearchParams? _searchParams;

    /// <summary>
    /// Creates a new URL object by parsing the input relative to the base.
    /// </summary>
    public URL(string input, string? @base = null)
    {
        if (@base != null)
        {
            var baseUri = new Uri(@base);
            _uri = new Uri(baseUri, input);
        }
        else
        {
            _uri = new Uri(input);
        }
    }

    /// <summary>
    /// Creates a new URL object by parsing the input relative to a base URL.
    /// </summary>
    public URL(string input, URL @base) : this(input, @base?.href ?? throw new ArgumentNullException(nameof(@base)))
    {
    }

    /// <summary>
    /// Gets or sets the serialized URL.
    /// </summary>
    public string href
    {
        get => _uri.ToString();
        set => _uri = new Uri(value);
    }

    /// <summary>
    /// Gets or sets the protocol scheme of the URL.
    /// </summary>
    public string protocol
    {
        get => _uri.Scheme + ":";
        set
        {
            var newValue = value.TrimEnd(':');
            var builder = new UriBuilder(_uri) { Scheme = newValue };
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets or sets the username portion of the URL.
    /// </summary>
    public string username
    {
        get
        {
            var userInfo = _uri.UserInfo;
            var colonIndex = userInfo.IndexOf(':');
            return colonIndex >= 0 ? userInfo.Substring(0, colonIndex) : userInfo;
        }
        set
        {
            var builder = new UriBuilder(_uri);
            var password = this.password;
            builder.UserName = value;
            if (!string.IsNullOrEmpty(password))
                builder.Password = password;
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets or sets the password portion of the URL.
    /// </summary>
    public string password
    {
        get
        {
            var userInfo = _uri.UserInfo;
            var colonIndex = userInfo.IndexOf(':');
            return colonIndex >= 0 ? userInfo.Substring(colonIndex + 1) : "";
        }
        set
        {
            var builder = new UriBuilder(_uri);
            builder.Password = value;
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets or sets the host portion of the URL (hostname + port).
    /// </summary>
    public string host
    {
        get => _uri.Port == -1 || IsDefaultPort() ? _uri.Host : $"{_uri.Host}:{_uri.Port}";
        set
        {
            var builder = new UriBuilder(_uri);
            var parts = value.Split(':', 2);
            builder.Host = parts[0];
            if (parts.Length > 1 && int.TryParse(parts[1], out int port))
            {
                builder.Port = port;
            }
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets or sets the hostname portion of the URL.
    /// </summary>
    public string hostname
    {
        get => _uri.Host;
        set
        {
            var builder = new UriBuilder(_uri) { Host = value };
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets or sets the port portion of the URL.
    /// </summary>
    public string port
    {
        get => _uri.Port == -1 || IsDefaultPort() ? "" : _uri.Port.ToString();
        set
        {
            var builder = new UriBuilder(_uri);
            builder.Port = string.IsNullOrEmpty(value) ? -1 : int.Parse(value);
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets or sets the path portion of the URL.
    /// </summary>
    public string pathname
    {
        get => _uri.AbsolutePath;
        set
        {
            var builder = new UriBuilder(_uri) { Path = value };
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets or sets the serialized query portion of the URL.
    /// </summary>
    public string search
    {
        get => string.IsNullOrEmpty(_uri.Query) ? "" : _uri.Query;
        set
        {
            var builder = new UriBuilder(_uri) { Query = value.TrimStart('?') };
            _uri = builder.Uri;
            _searchParams = null; // Invalidate cached searchParams
        }
    }

    /// <summary>
    /// Gets a URLSearchParams object representing the query parameters of the URL.
    /// </summary>
    public URLSearchParams searchParams
    {
        get
        {
            if (_searchParams == null)
            {
                _searchParams = new URLSearchParams(_uri.Query);
            }
            return _searchParams;
        }
    }

    /// <summary>
    /// Gets or sets the fragment portion of the URL.
    /// </summary>
    public string hash
    {
        get => string.IsNullOrEmpty(_uri.Fragment) ? "" : _uri.Fragment;
        set
        {
            var builder = new UriBuilder(_uri) { Fragment = value.TrimStart('#') };
            _uri = builder.Uri;
        }
    }

    /// <summary>
    /// Gets the read-only origin of the URL.
    /// </summary>
    public string origin => $"{_uri.Scheme}://{host}";

    /// <summary>
    /// Returns the serialized URL as a string.
    /// </summary>
    public override string ToString() => href;

    /// <summary>
    /// Returns the serialized URL as a string (for JSON serialization).
    /// </summary>
    public string toJSON() => href;

    /// <summary>
    /// Tests if input can be parsed as a URL.
    /// </summary>
    public static bool canParse(string input, string? @base = null)
    {
        try
        {
            if (@base != null)
            {
                var baseUri = new Uri(@base);
                new Uri(baseUri, input);
            }
            else
            {
                new Uri(input);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Tests if input can be parsed as a URL relative to a base URL.
    /// </summary>
    public static bool canParse(string input, URL @base)
    {
        if (@base == null)
            throw new ArgumentNullException(nameof(@base));

        return canParse(input, @base.href);
    }

    /// <summary>
    /// Parses a URL string and returns a URL object, or null if parsing fails.
    /// </summary>
    public static URL? parse(string input, string? @base = null)
    {
        try
        {
            return new URL(input, @base);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parses a URL string relative to a base URL, or returns null if parsing fails.
    /// </summary>
    public static URL? parse(string input, URL @base)
    {
        if (@base == null)
            throw new ArgumentNullException(nameof(@base));

        return parse(input, @base.href);
    }

    private bool IsDefaultPort()
    {
        return (_uri.Scheme == "http" && _uri.Port == 80) ||
               (_uri.Scheme == "https" && _uri.Port == 443) ||
               (_uri.Scheme == "ftp" && _uri.Port == 21);
    }
}
