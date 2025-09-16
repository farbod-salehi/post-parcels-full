using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace API.Helpers;

public class Constances
{

    public struct JWTSettings
    {
        public static string Key => "7C90024B-F405-40E7-BB18-AAA1FAE7789E";
        public static int ValidDays => 30;
        public static string Issuer => "PostParcelAPI";
        public static string Audience => "PostParcelApp";

    }

    public enum UserRole
    {
        admin = 1,
        manager,
        user
    }

    public enum ParcelItemType
    {
        Case = 1,
        Letter,
        Docs,
        Zonekan,
        Carton,
        Sack // گونی
    }

    public static string GetParcelItemTypeTitle(int typeValue)
    {
        return typeValue switch
        {
            (int)ParcelItemType.Case => "پرونده",
            (int)ParcelItemType.Letter => "نامه",
            (int)ParcelItemType.Docs => "اوراق",
            (int)ParcelItemType.Zonekan => "زونکن",
            (int)ParcelItemType.Carton => "کارتن",
            (int)ParcelItemType.Sack => "گونی",
            _ => ""
        };

    }

    public static string[] GetImagesValidExtensions()
    {
        return [".jpg", ".jpeg", ".png"];
    }

    public static string[] GetDocumentValidExtensions()
    {
        return [".pdf"];
    }

    public static int UserPasswordMinLength { get { return 6; } }

    public static string DevelopmentConnectionString
    {
        get
        {
            return "data source=.; initial catalog=PostParcel;user id=sa;password=asdf;MultipleActiveResultSets=True;TrustServerCertificate=True";
        }
    }

    public static string ProductionConnectionString
    {
        get
        {
            return "data source=.; initial catalog=PostParcel;user id=sa;password=asdf;MultipleActiveResultSets=True;TrustServerCertificate=True";
        }
    }
}

