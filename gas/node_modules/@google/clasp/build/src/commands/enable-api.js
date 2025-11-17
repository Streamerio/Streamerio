// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// This file defines the 'enable-api' command for the clasp CLI.
import { Command } from 'commander';
import { intl } from '../intl.js';
import { assertGcpProjectConfigured, maybePromptForProjectId, withSpinner } from './utils.js';
export const command = new Command('enable-api')
    .description('Enable a service for the current project.')
    .argument('<api>', 'Service to enable')
    .action(async function (serviceName) {
    var _a;
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    await maybePromptForProjectId(clasp);
    assertGcpProjectConfigured(clasp);
    try {
        const spinnerMsg = intl.formatMessage({ id: "UqhzFn", defaultMessage: [{ type: 0, value: "Enabling service..." }] });
        await withSpinner(spinnerMsg, async () => {
            await clasp.services.enableService(serviceName);
        });
    }
    catch (error) {
        if (((_a = error.cause) === null || _a === void 0 ? void 0 : _a.code) === 'NOT_AUTHORIZED') {
            const msg = intl.formatMessage({ id: "+y0rAf", defaultMessage: [{ type: 0, value: "Not authorized to enable " }, { type: 1, value: "name" }, { type: 0, value: " or it does not exist." }] }, {
                name: serviceName,
            });
            this.error(msg);
        }
        throw error;
    }
    if (options.json) {
        console.log(JSON.stringify({ success: true }, null, 2));
        return;
    }
    const successMessage = intl.formatMessage({ id: "6lXgOl", defaultMessage: [{ type: 0, value: "Enabled " }, { type: 1, value: "name" }, { type: 0, value: " API." }] }, {
        name: serviceName,
    });
    console.log(successMessage);
});
