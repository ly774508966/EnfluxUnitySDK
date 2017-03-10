// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System.Text.RegularExpressions;
using UnityEngine;

namespace Enflux.SDK.Utils
{
    public static class RigMapperUtils
    {
        private static readonly string[] StandardCoreNames =
        {
            "core"
        };

        private static readonly string[] StandardUpperArmNames =
        {
            "shldr",
            "shoulder"
        };

        private static readonly string[] StandardLowerArmNames =
        {
            "forearm",
            "elbow"
        };

        private static readonly string[] StandardWaistNames =
        {
            "waist"
        };

        private static readonly string[] StandardUpperLegNames =
        {
            "thigh",
            "leg"
        };

        private static readonly string[] StandardLowerLegNames =
        {
            "shin",
            "knee"
        };

        private static readonly string[] StandardLeftNames =
        {
            "l",
            "left",
            "lft",
            "lt"
        };

        private static readonly string[] StandardRightNames =
        {
            "r",
            "right",
            "rgt",
            "rt"
        };

        private static readonly string[] StandardCenterNames =
        {
            "",
            "center",
            "cen"
        };

        private const string RegexFormatString = "{0}[\\ _\\-]*{1}";


        public static Transform ResolveCore(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardCoreNames, StandardCenterNames);
        }

        public static Transform ResolveLeftUpperArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperArmNames, StandardLeftNames);
        }

        public static Transform ResolveLeftLowerArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerArmNames, StandardLeftNames);
        }

        public static Transform ResolveRightUpperArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperArmNames, StandardRightNames);
        }

        public static Transform ResolveRightLowerArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerArmNames, StandardRightNames);
        }

        public static Transform ResolveWaist(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardWaistNames, StandardCenterNames);
        }

        public static Transform ResolveLeftUpperLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperLegNames, StandardLeftNames);
        }

        public static Transform ResolveLeftLowerLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerLegNames, StandardLeftNames);
        }

        public static Transform ResolveRightUpperLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperLegNames, StandardRightNames);
        }

        public static Transform ResolveRightLowerLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerLegNames, StandardRightNames);
        }

        /// <summary>
        /// Returns a child transform by taking a list of possible names, a list of possible prefixes/suffixes, and checking if the child transform's name matches a possible pattern. Allows a hypens, spaces, and underscores between the limb name and prefix/suffix. Doesn't check if capitalization is the same.
        ///
        /// Note that this method is not performant, as it greedily allocates memory as needed for checking each transform. 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="possibleLimbNames"></param>
        /// <param name="possibleDirectionNames">List of possible prefixes or suffixes</param>
        /// <returns></returns>
        private static Transform ResolveLimbTransformByName(this Transform root, string[] possibleLimbNames, string[] possibleDirectionNames)
        {
            if (root == null)
            {
                return null;
            }
            var children = root.GetComponentsInChildren<Transform>(true);
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < children.Length; ++i)
            {
                var childName = children[i].name.ToLower();
                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var j = 0; j < possibleLimbNames.Length; ++j)
                {
                    var limbName = possibleLimbNames[j];
                    for (var k = 0; k < possibleDirectionNames.Length; ++k)
                    {
                        var directionName = possibleDirectionNames[k];

                        // Check for directiom name then "", " ", "_", or "-" then limb name.
                        var prefixRegex = new Regex(string.Format(RegexFormatString, directionName, limbName));
                        if (prefixRegex.IsMatch(childName))
                        {
                            return children[i];
                        }
                        // Check for limb name then "", " ", "_", or "-" then direction name.
                        var suffixRegex = new Regex(string.Format(RegexFormatString, limbName, directionName));
                        if (suffixRegex.IsMatch(childName))
                        {
                            return children[i];
                        }
                    }
                }
            }
            return null;
        }
    }
}