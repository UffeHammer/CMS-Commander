using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
 
namespace SitecoreConverter.Core.Plugins
{
    public class PluginManager
    {

        private static PluginManagerConfiguration _configs = null;
        public static PluginManagerConfiguration Configs
        {
            get
            {
                if (_configs == null)
                {
                    _configs = System.Configuration.ConfigurationManager.GetSection("PluginConfiguration")
                            as PluginManagerConfiguration;
                }
                return _configs;
            }
        }

        private static IItemCopyPlugin[] _ItemCopyPlugins = null;
        public static IItemCopyPlugin[] ItemCopyPlugins
        {
            get
            {
                if (_ItemCopyPlugins == null)
                {
                    _ItemCopyPlugins = new IItemCopyPlugin[Configs.ItemCopyPlugins.Count];
                    for (int t = 0; t < Configs.ItemCopyPlugins.Count; t++)
                    {
                        PluginConfigurationElement configElement = Configs.ItemCopyPlugins[t];
                        _ItemCopyPlugins[t] = (IItemCopyPlugin)loadPlugin(configElement.Assembly, configElement.PluginType);                        
                    }
                }
                return _ItemCopyPlugins;
            }
        }


        private static object loadPlugin(string sAssemblyFileName, string sPluginType)
        {
            object returnObject = null;
            try
            {
                /* Initialize some standard objects
                 * that will be used to load up the
                 * assembly.
                 */
                Assembly assembly = null;
                string typeName = string.Empty;
                Type pluginType = null;

                sAssemblyFileName = sAssemblyFileName + ".dll";
                

                if (File.Exists(sAssemblyFileName))
                {
                    assembly = Assembly.LoadFile(Path.GetFullPath(sAssemblyFileName));
                }
                else
                {
                    /* The file that was specified does not exist.
                     * Checking here is much faster than waiting
                     * until the catch block to handle an
                     * FileDoesNotExist exception.  Substitute the
                     * error message code with logging code, etc.
                     */

                    throw new Exception(string.Format("The specified file '{0}' does not exist.", sAssemblyFileName));
                }

                if (null != assembly)
                {
                    pluginType = assembly.GetType(sPluginType, true);

                    /* Make sure before trying to act
                     * on the object that a type
                     * was actually found.
                     */
                    if (null != pluginType)
                    {
                        returnObject = Activator.CreateInstance(pluginType);
                    }
                    else
                    {
                        throw new ApplicationException
                      ("The plugin does not contain the correct type.");
                    }
                }

            }
            catch (System.InvalidCastException)
            {
                /* The object didn't implement the
                 * IAuthenticationPlugin interface, and could
                 * not be cast correctly */
                throw new Exception
                    ("The authentication object does not implement the correct interface.");
            }
            catch (System.BadImageFormatException)
            {
                /* The Assembly is invalid
                 * It is not a valid .NET Assembly file. */
                throw new Exception(string.Format
                   ("Invalid plugin file '{0}'", sAssemblyFileName));
            }
            return returnObject;
        }
    }
}
