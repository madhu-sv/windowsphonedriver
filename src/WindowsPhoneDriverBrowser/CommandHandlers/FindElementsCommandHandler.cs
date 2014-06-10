﻿// <copyright file="FindElementsCommandHandler.cs" company="Salesforce.com">
//
// Copyright (c) 2014 Salesforce.com, Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
// following conditions are met:
//
//    Redistributions of source code must retain the above copyright notice, this list of conditions and the following
//    disclaimer.
//
//    Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
//    following disclaimer in the documentation and/or other materials provided with the distribution.
//
//    Neither the name of Salesforce.com nor the names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
// USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WindowsPhoneDriverBrowser.CommandHandlers
{
    /// <summary>
    /// Provides handling for the find elements command.
    /// </summary>
    internal class FindElementsCommandHandler : CommandHandler
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="environment">The <see cref="CommandEnvironment"/> to use in executing the command.</param>
        /// <param name="parameters">The <see cref="Dictionary{string, object}"/> containing the command parameters.</param>
        /// <returns>The JSON serialized string representing the command response.</returns>
        public override Response Execute(CommandEnvironment environment, Dictionary<string, object> parameters)
        {
            object mechanism;
            if (!parameters.TryGetValue("using", out mechanism))
            {
                return Response.CreateMissingParametersResponse("using");
            }

            object criteria;
            if (!parameters.TryGetValue("value", out criteria))
            {
                return Response.CreateMissingParametersResponse("value");
            }

            Response response;
            DateTime timeout = DateTime.Now.AddMilliseconds(environment.ImplicitWaitTimeout);
            do
            {
                string result = this.EvaluateAtom(environment, WebDriverAtoms.FindElements, mechanism, criteria, null, environment.CreateFrameObject());
                response = Response.FromJson(result);
                if (response.Status == WebDriverStatusCode.Success)
                {
                    object[] foundElements = response.Value as object[];
                    if (foundElements != null && foundElements.Length > 0)
                    {
                        // Return early for success
                        return response;
                    }
                }
                else if (response.Status != WebDriverStatusCode.NoSuchElement)
                {
                    if (mechanism.ToString().ToUpperInvariant() != "XPATH" && response.Status == WebDriverStatusCode.InvalidSelector)
                    {
                        response.Status = WebDriverStatusCode.NoSuchElement;
                    }

                    // Also return early for response of not NoSuchElement.
                    return response;
                }
            }
            while (DateTime.Now < timeout);

            // We should theoretically only reach here if the timeout has
            // expired, and no elements have been found. This is still a
            // success condition. Since the JSON result hasn't actually been
            // modified, we can simply return it.
            return response;
        }
    }
}
