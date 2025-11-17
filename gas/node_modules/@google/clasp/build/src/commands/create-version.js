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
// This file defines the 'create-version' (alias 'version') command for the
// clasp CLI.
import { Command } from 'commander';
import inquirer from 'inquirer';
import { intl } from '../intl.js';
import { isInteractive, withSpinner } from './utils.js';
export const command = new Command('create-version')
    .alias('version')
    .arguments('[description]')
    .description('Creates an immutable version of the script')
    .action(async function (description) {
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    if (!description && isInteractive()) {
        const prompt = intl.formatMessage({ id: "6U9ksF", defaultMessage: [{ type: 0, value: "Give a description:" }] });
        const answer = await inquirer.prompt([
            {
                default: '',
                message: prompt,
                name: 'description',
                type: 'input',
            },
        ]);
        description = answer.description;
    }
    const spinnerMsg = intl.formatMessage({ id: "HhBwsB", defaultMessage: [{ type: 0, value: "Creating a new version..." }] });
    const versionNumber = await withSpinner(spinnerMsg, async () => {
        return clasp.project.version(description);
    });
    if (options.json) {
        console.log(JSON.stringify({ versionNumber }, null, 2));
        return;
    }
    const successMessage = intl.formatMessage({ id: "TVOEZz", defaultMessage: [{ type: 0, value: "Created version " }, { type: 2, value: "version", style: null }] }, {
        version: versionNumber,
    });
    console.log(successMessage);
});
