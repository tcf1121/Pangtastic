// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("e6sQngMjrpeapxWyTnCHQvh4T1SREhwTI5ESGRGREhITrjiYYalCojxf7I4oV8sxIOz+Zh4yJz70zNpmOyUmxJMtPblusjHWZ1FxAq9CkMfVv3cLL1AJyPEXs1RMoCwvJoPZahOP3Wj/ebbeGLfDyrEl7FCeC99C8XsiCAbEsnFK/NJrJCdV75484BZgOpcSK0okCLuGHM4GhihQmr4f/iOREjEjHhUaOZVbleQeEhISFhMQCbMYHjqnhGSqTtctkYDz16x3X/Xmj9msSrQEV4nEOdBfmdv3wXyHp5I41ypVYIXSb2K+hduQDUq050cgniFYCTvpVwDxcARmY+dwV776/0mxWehJcKTba4C9yWgfCYJ2TayvK9VDN8RbXAdpJhEQEhMS");
        private static int[] order = new int[] { 7,1,4,13,6,6,8,8,8,9,12,12,12,13,14 };
        private static int key = 19;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
