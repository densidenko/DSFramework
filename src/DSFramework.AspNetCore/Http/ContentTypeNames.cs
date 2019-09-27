using System;

namespace DSFramework.AspNetCore.Http
{
    /// <summary>
    ///     A list of internet media types, which are a standard identifier used on the Internet to indicate the type of
    ///     data that a file contains. Web browsers use them to determine how to display, output or handle files and search
    ///     engines use them to classify data files on the web.
    /// </summary>
    public static class ContentTypeNames
    {
        ///<summary>Used to denote the encoding necessary for files containing JavaScript source code. The alternative MIME type for this file type is text/javascript.</summary>
        public const string APPLICATION_X_JAVASCRIPT = "application/x-javascript";

        ///<summary>24bit Linear PCM audio at 8-48kHz, 1-N channels; Defined in RFC 3190</summary>
        public const string AUDIO_L24 = "audio/L24";

        ///<summary>Adobe Flash files for example with the extension .swf</summary>
        public const string APPLICATION_X_SHOCKWAVE_FLASH = "application/x-shockwave-flash";

        ///<summary>Arbitrary binary data.[5] Generally speaking this type identifies files that are not associated with a specific application. Contrary to past assumptions by software packages such as Apache this is not a type that should be applied to unknown files. In such a case, a server or application should not indicate a content type, as it may be incorrect, but rather, should omit the type in order to allow the recipient to guess the type.[6]</summary>
        public const string APPLICATION_OCTET_STREAM = "application/octet-stream";

        ///<summary>Atom feeds</summary>
        public const string APPLICATION_ATOM_XML = "application/atom+xml";

        ///<summary>Cascading Style Sheets; Defined in RFC 2318</summary>
        public const string TEXT_CSS = "text/css";

        ///<summary>commands; subtype resident in Gecko browsers like Firefox 3.5</summary>
        public const string TEXT_CMD = "text/cmd";

        ///<summary>Comma-separated values; Defined in RFC 4180</summary>
        public const string TEXT_CSV = "text/csv";

        ///<summary>deb (file format), a software package format used by the Debian project</summary>
        public const string APPLICATION_X_DEB = "application/x-deb";

        ///<summary>Defined in RFC 1847</summary>
        public const string MULTIPART_ENCRYPTED = "multipart/encrypted";

        ///<summary>Defined in RFC 1847</summary>
        public const string MULTIPART_SIGNED = "multipart/signed";

        ///<summary>Defined in RFC 2616</summary>
        public const string MESSAGE_HTTP = "message/http";

        ///<summary>Defined in RFC 4735</summary>
        public const string MODEL_EXAMPLE = "model/example";

        ///<summary>device-independent document in DVI format</summary>
        public const string APPLICATION_X_DVI = "application/x-dvi";

        ///<summary>DTD files; Defined by RFC 3023</summary>
        public const string APPLICATION_XML_DTD = "application/xml-dtd";

        ///<summary>ECMAScript/JavaScript; Defined in RFC 4329 (equivalent to application/ecmascript but with looser processing rules) It is not accepted in IE 8 or earlier - text/javascript is accepted but it is defined as obsolete in RFC 4329. The "type" attribute of the <script> tag in HTML5 is optional and in practice omitting the media type of JavaScript programs is the most interoperable solution since all browsers have always assumed the correct default even before HTML5.</summary>
        public const string APPLICATION_JAVASCRIPT = "application/javascript";

        ///<summary>ECMAScript/JavaScript; Defined in RFC 4329 (equivalent to application/javascript but with stricter processing rules)</summary>
        public const string APPLICATION_ECMASCRIPT = "application/ecmascript";

        ///<summary>EDI EDIFACT data; Defined in RFC 1767</summary>
        public const string APPLICATION_EDIFACT = "application/EDIFACT";

        ///<summary>EDI X12 data; Defined in RFC 1767</summary>
        public const string APPLICATION_EDI_X12 = "application/EDI-X12";

        ///<summary>Email; Defined in RFC 2045 and RFC 2046</summary>
        public const string MESSAGE_PARTIAL = "message/partial";

        ///<summary>Email; EML files, MIME files, MHT files, MHTML files; Defined in RFC 2045 and RFC 2046</summary>
        public const string MESSAGE_RFC822 = "message/rfc822";

        ///<summary>Extensible Markup Language; Defined in RFC 3023</summary>
        public const string TEXT_XML = "text/xml";

        ///<summary>Flash video (FLV files)</summary>
        public const string VIDEO_X_FLV = "video/x-flv";

        ///<summary>GIF image; Defined in RFC 2045 and RFC 2046</summary>
        public const string IMAGE_GIF = "image/gif";

        ///<summary>GoogleWebToolkit data</summary>
        public const string TEXT_X_GWT_RPC = "text/x-gwt-rpc";

        ///<summary>Gzip</summary>
        public const string APPLICATION_X_GZIP = "application/x-gzip";

        ///<summary>HTML; Defined in RFC 2854</summary>
        public const string TEXT_HTML = "text/html";

        ///<summary>ICO image; Registered[9]</summary>
        public const string IMAGE_VND_MICROSOFT_ICON = "image/vnd.microsoft.icon";

        ///<summary>IGS files, IGES files; Defined in RFC 2077</summary>
        public const string MODEL_IGES = "model/iges";

        ///<summary>IMDN Instant Message Disposition Notification; Defined in RFC 5438</summary>
        public const string MESSAGE_IMDN_XML = "message/imdn+xml";

        ///<summary>JavaScript Object Notation JSON; Defined in RFC 4627</summary>
        public const string APPLICATION_JSON = "application/json";

        ///<summary>JavaScript Object Notation (JSON) Patch; Defined in RFC 6902</summary>
        public const string APPLICATION_JSON_PATCH = "application/json-patch+json";

        ///<summary>JavaScript - Defined in and obsoleted by RFC 4329 in order to discourage its usage in favor of application/javascript. However,text/javascript is allowed in HTML 4 and 5 and, unlike application/javascript, has cross-browser support. The "type" attribute of the <script> tag in HTML5 is optional and there is no need to use it at all since all browsers have always assumed the correct default (even in HTML 4 where it was required by the specification).</summary>
        [Obsolete]
        public const string TEXT_JAVASCRIPT = "text/javascript";

        ///<summary>JPEG JFIF image; Associated with Internet Explorer; Listed in ms775147(v=vs.85) - Progressive JPEG, initiated before global browser support for progressive JPEGs (Microsoft and Firefox).</summary>
        public const string IMAGE_PJPEG = "image/pjpeg";

        ///<summary>JPEG JFIF image; Defined in RFC 2045 and RFC 2046</summary>
        public const string IMAGE_JPEG = "image/jpeg";

        ///<summary>jQuery template data</summary>
        public const string TEXT_X_JQUERY_TMPL = "text/x-jquery-tmpl";

        ///<summary>KML files (e.g. for Google Earth)</summary>
        public const string APPLICATION_VND_GOOGLE_EARTH_KML_XML = "application/vnd.google-earth.kml+xml";

        ///<summary>Matroska open media format</summary>
        public const string VIDEO_X_MATROSKA = "video/x-matroska";
        
        ///<summary>Microsoft Excel files</summary>
        public const string APPLICATION_VND_MS_EXCEL = "application/vnd.ms-excel";

        ///<summary>MIME Email; Defined in RFC 2045 and RFC 2046</summary>
        public const string MULTIPART_MIXED = "multipart/mixed";

        ///<summary>MIME Email; Defined in RFC 2387 and used by MHTML (HTML mail)</summary>
        public const string MULTIPART_RELATED = "multipart/related";

        ///<summary>MIME Webform; Defined in RFC 2388</summary>
        public const string MULTIPART_FORM_DATA = "multipart/form-data";
        
        ///<summary>MP3 or other MPEG audio; Defined in RFC 3003</summary>
        public const string AUDIO_MPEG = "audio/mpeg";

        ///<summary>MP4 audio</summary>
        public const string AUDIO_MP4 = "audio/mp4";

        ///<summary>MP4 video; Defined in RFC 4337</summary>
        public const string VIDEO_MP4 = "video/mp4";

        ///<summary>MPEG-1 video with multiplexed audio; Defined in RFC 2045 and RFC 2046</summary>
        public const string VIDEO_MPEG = "video/mpeg";

        ///<summary>MSH files, MESH files; Defined in RFC 2077, SILO files</summary>
        public const string MODEL_MESH = "model/mesh";

        ///<summary>mulaw audio at 8 kHz, 1 channel; Defined in RFC 2046</summary>
        public const string AUDIO_BASIC = "audio/basic";

        ///<summary>Portable Document Format, PDF has been in use for document exchange on the Internet since 1993; Defined in RFC 3778</summary>
        public const string APPLICATION_PDF = "application/pdf";

        ///<summary>Portable Network Graphics; Registered,[8] Defined in RFC 2083</summary>
        public const string IMAGE_PNG = "image/png";

        ///<summary>PostScript; Defined in RFC 2046</summary>
        public const string APPLICATION_POSTSCRIPT = "application/postscript";

        ///<summary>QuickTime video; Registered[10]</summary>
        public const string VIDEO_QUICKTIME = "video/quicktime";

        ///<summary>RAR archive files</summary>
        public const string APPLICATION_X_RAR_COMPRESSED = "application/x-rar-compressed";

        ///<summary>RealAudio; Documented in RealPlayer Customer Support Answer 2559</summary>
        public const string AUDIO_VND_RN_REALAUDIO = "audio/vnd.rn-realaudio";

        ///<summary>Resource Description Framework; Defined by RFC 3870</summary>
        public const string APPLICATION_RDF_XML = "application/rdf+xml";
  
        ///<summary>SVG vector image; Defined in SVG Tiny 1.2 Specification Appendix M</summary>
        public const string IMAGE_SVG_XML = "image/svg+xml";

        ///<summary>Tarball files</summary>
        public const string APPLICATION_X_TAR = "application/x-tar";

        ///<summary>Textual data; Defined in RFC 2046 and RFC 3676</summary>
        public const string TEXT_PLAIN = "text/plain";

        ///<summary>WebM Matroska-based open media format</summary>
        public const string VIDEO_WEBM = "video/webm";

        ///<summary>WebM open media format</summary>
        public const string AUDIO_WEBM = "audio/webm";

        ///<summary>Windows Media Audio; Documented in Microsoft KB 288102</summary>
        public const string AUDIO_X_MS_WMA = "audio/x-ms-wma";

        ///<summary>Windows Media Video; Documented in Microsoft KB 288102</summary>
        public const string VIDEO_X_MS_WMV = "video/x-ms-wmv";

        ///<summary>XHTML; Defined by RFC 3236</summary>
        public const string APPLICATION_XHTML_XML = "application/xhtml+xml";

        ///<summary>ZIP archive files; Registered[7]</summary>
        public const string APPLICATION_ZIP = "application/zip";
    }
}