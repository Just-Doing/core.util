import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.security.KeyFactory;
import java.security.PublicKey;
import java.security.cert.CertificateException;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;
import java.security.interfaces.RSAPrivateKey;
import java.security.interfaces.RSAPublicKey;
import java.security.spec.PKCS8EncodedKeySpec;
import java.util.Base64;

import javax.crypto.Cipher;
import javax.print.DocFlavor.URL;

public class TestRsaEntry {


    public static void main(String[] args) throws Exception {
        String pubKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCG77PYUAcCpANyUmsHJfuDIia9FcITsuu9lnfbE2BbEwd4SOxPBFTwEWTZ9/e+mtjP97KFEBohGkwy+VHE5KocypBv0O7YgEevwMgpvxyYY0v104CB/k0yjCFV7lc7FxY5VgEKrMiXTIkMr1ukCnWVvapvRCS6IFcsT/kkjPgfDQIDAQAB";
        String str = encrypt("test1111111111111111111", pubKey );
        System.out.print(str);
        
        /*
         * String privateKey =
         * "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBAIbvs9hQBwKkA3JSawcl+4MiJr0VwhOy672Wd9sTYFsTB3hI7E8EVPARZNn3976a2M/3soUQGiEaTDL5UcTkqhzKkG/Q7tiAR6/AyCm/HJhjS/XTgIH+TTKMIVXuVzsXFjlWAQqsyJdMiQyvW6QKdZW9qm9EJLogVyxP+SSM+B8NAgMBAAECgYEAhj0FH9dNghUE0MCpdS0WL/jTrRxuPQase6mrhyiZnUErF0EExf87OLE1MZr8voRx2UNEOBgyxmfREozyCfyqNg1OdGYEHSyuJ9wglkhq8GVYO8IzI29Mqej0MSprtsE0BPAKBHRU/DWP19ej5bv5ZnAhLs10K7uVEsuGwJJYcMECQQDibedUr7tnGfojyjFY0vCAaVwgS0vXfno7WQyAXUz0Fv8Uy1q9nyF0RrkeA8BOk7S4ljE77ufX0rr2qL7kHW8pAkEAmI718EnQCKKJUjrQUl4iG/lYoNwW2QnxTGZmESyFwkS95PTt8K4GVHpICqRNP1JJBNxVSEVts/eA4zrxPAoBRQJBAJxxEsOQJwq1B/5yVGXqWABgyyYE4AGjgRBAFkMaM3Dx8ouLdMZOi+6qbnwuW0/u/Y4LNzkRd13GWybQsBMrwwECQEULptmavpG55kaWIcS1n+BjSK59DcYrDs+SJK2vJdaXwA4IoEvmpyzCrypJ1EBNYIjXo61y5sSlxuqQua9/o7UCQGYdM3/mF/FEC3wxdfQq0Pw/Pwn8RQxg1natRfoTyzOJDfE/YUYGjIEe2pQtDI1s+IRCwrXOB0cySbpaSHCjr5U=";
         * String decStr = decrypt(str, privateKey); System.out.print(decStr);
         */
    }
    
    public static String encrypt( String str, String publicKey ) throws Exception{
        Base64.Encoder encoder = Base64.getEncoder();
        Base64.Decoder decoder = Base64.getDecoder();
        //base64编码的公钥
        /*
         * byte[] decoded = decoder.decode(publicKey); RSAPublicKey pubKey =
         * (RSAPublicKey) KeyFactory.getInstance("RSA").generatePublic(new
         * X509EncodedKeySpec(decoded));
         */
        PublicKey pubKey = GetPubkey();
        //RSA加密
        Cipher cipher = Cipher.getInstance("RSA");
        cipher.init(Cipher.ENCRYPT_MODE, pubKey);
        String outStr = encoder.encodeToString(cipher.doFinal(str.getBytes("UTF-8")));
        return outStr;
    }
    
    
    public static String decrypt(String str, String privateKey) throws Exception{
        Base64.Decoder decoder = Base64.getDecoder();
        //64位解码加密后的字符串
        byte[] inputByte = decoder.decode(str.getBytes("UTF-8"));
        //base64编码的私钥
        byte[] decoded = decoder.decode(privateKey);  
        RSAPrivateKey priKey = (RSAPrivateKey) KeyFactory.getInstance("RSA").generatePrivate(new PKCS8EncodedKeySpec(decoded));  
        //RSA解密
        Cipher cipher = Cipher.getInstance("RSA");
        cipher.init(Cipher.DECRYPT_MODE, priKey);
        String outStr = new String(cipher.doFinal(inputByte));
        return outStr;
    }
    
    public static PublicKey GetPubkey() throws CertificateException, FileNotFoundException
    {
   
        File certFile = new File("D:\\\\cert\\\\taikang_public.crt");
        CertificateFactory cf = CertificateFactory.getInstance("X.509");
        X509Certificate cert = (X509Certificate)cf.generateCertificate(new FileInputStream(certFile));
        

        PublicKey publicKey = cert.getPublicKey();
        return publicKey;
    }

}