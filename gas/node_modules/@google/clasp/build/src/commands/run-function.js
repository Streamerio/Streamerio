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
// This file defines the 'run-function' (alias 'run') command for the clasp CLI.
import chalk from 'chalk';
import { Command } from 'commander';
import fuzzy from 'fuzzy';
import autocomplete from 'inquirer-autocomplete-standalone';
import { intl } from '../intl.js';
import { isInteractive, withSpinner } from './utils.js';
export const command = new Command('run-function')
    .alias('run')
    .description('Run a function in your Apps Scripts project')
    .argument('[functionName]', 'The name of the function to run')
    .option('--nondev', 'Run script function in non-devMode')
    .option('-p, --params <value>', 'Parameters to pass to the function, as a JSON-encoded array')
    .action(async function (functionName) {
    var _a, _b, _c;
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    const devMode = !options.nondev; // Defaults to true
    let params = [];
    if (options.params) {
        // Parameters for the function are expected to be a JSON-encoded array.
        params = JSON.parse(options.params);
    }
    // If no function name is provided and the session is interactive,
    // fetch all function names from the project and prompt the user to select one.
    if (!functionName && isInteractive()) {
        const allFunctions = await clasp.functions.getFunctionNames();
        // `inquirer-autocomplete-standalone` provides a fuzzy-searchable list.
        const source = async (input = '') => fuzzy.filter(input, allFunctions).map(element => ({
            value: element.original, // The original function name is the value.
        }));
        const prompt = intl.formatMessage({ id: "Y8u3Vb", defaultMessage: [{ type: 0, value: "Selection a function name" }] });
        functionName = await autocomplete({
            message: prompt,
            source, // Source function for the autocomplete.
        });
    }
    // Attempt to run the function.
    try {
        // `clasp.functions.runFunction` calls the Apps Script API.
        const result = await withSpinner(`Running function: ${functionName}`, async () => {
            return clasp.functions.runFunction(functionName, params, devMode);
        });
        if (options.json) {
            const output = {
                response: (_a = result.response) === null || _a === void 0 ? void 0 : _a.result,
                error: result.error
                    ? {
                        code: result.error.code,
                        message: result.error.message,
                        details: result.error.details,
                    }
                    : undefined,
            };
            console.log(JSON.stringify(output, null, 2));
            return;
        }
        // Handle the API response.
        if (result.error && result.error.details) {
            // If the API returned an error in the `error.details` field (common for script execution errors).
            const { errorMessage, scriptStackTraceElements } = result.error.details[0];
            const msg = intl.formatMessage({ id: "1l462L", defaultMessage: [{ type: 0, value: "Exception:" }] });
            console.error(`${chalk.red(msg)}`, errorMessage, scriptStackTraceElements || []);
            return;
        }
        if (result.response && result.response.result !== undefined) {
            // If the function executed successfully and returned a result.
            console.log(result.response.result);
        }
        else {
            // If the function execution didn't produce a result or an error in the expected format.
            const msg = intl.formatMessage({ id: "S/0IKk", defaultMessage: [{ type: 0, value: "No response." }] });
            console.log(chalk.red(msg));
        }
    }
    catch (error) {
        // Handle errors thrown by `clasp.functions.runFunction` or other issues.
        if (((_b = error.cause) === null || _b === void 0 ? void 0 : _b.code) === 'NOT_AUTHORIZED') {
            // Specific error for lack of permissions.
            const msg = intl.formatMessage({ id: "HZmND2", defaultMessage: [{ type: 0, value: "Unable to run script function. Please make sure you have permission to run the script function." }] });
            this.error(msg);
        }
        if (((_c = error.cause) === null || _c === void 0 ? void 0 : _c.code) === 'NOT_FOUND') {
            // Specific error if the function or script (as API executable) is not found.
            const msg = intl.formatMessage({ id: "4wxpit", defaultMessage: [{ type: 0, value: "Script function not found. Please make sure script is deployed as API executable." }] });
            this.error(msg);
        }
        // Re-throw other errors to be caught by the global error handler.
        throw error;
    }
});
