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
// This file defines the 'list-versions' (alias 'versions') command for the
// clasp CLI.
import { Command } from 'commander';
import { intl } from '../intl.js';
import { withSpinner } from './utils.js';
export const command = new Command('list-versions')
    .alias('versions')
    .description('List versions of a script')
    .argument('[scriptId]', 'Apps Script ID to list deployments for')
    .action(async function (scriptId) {
    var _a;
    const options = this.optsWithGlobals();
    const clasp = options.clasp;
    if (scriptId) {
        clasp.withScriptId(scriptId);
    }
    const spinnerMsg = intl.formatMessage({ id: "Cqxqh0", defaultMessage: [{ type: 0, value: "Fetching versions..." }] });
    const versions = await withSpinner(spinnerMsg, () => clasp.project.listVersions());
    if (options.json) {
        const versionOutput = versions.results.map(version => ({
            versionNumber: version.versionNumber,
            description: version.description,
        }));
        console.log(JSON.stringify(versionOutput, null, 2));
        return;
    }
    if (!((_a = versions.results) === null || _a === void 0 ? void 0 : _a.length)) {
        const msg = intl.formatMessage({ id: "Jmvq7L", defaultMessage: [{ type: 0, value: "No deployed versions of script." }] });
        console.log(msg);
        return;
    }
    const successMessage = intl.formatMessage({ id: "6CS9SE", defaultMessage: [{ type: 0, value: "Found " }, { type: 6, value: "count", options: { one: { value: [{ type: 7 }, { type: 0, value: " version" }] }, other: { value: [{ type: 7 }, { type: 0, value: " versions" }] } }, offset: 0, pluralType: "cardinal" }, { type: 0, value: "." }] }, {
        count: versions.results.length,
    });
    console.log(successMessage);
    versions.results.reverse();
    versions.results.forEach(version => {
        const msg = intl.formatMessage({ id: "CAt8s3", defaultMessage: [{ type: 2, value: "version", style: null }, { type: 0, value: " - " }, { type: 5, value: "description", options: { undefined: { value: [{ type: 0, value: "No description" }] }, other: { value: [{ type: 1, value: "description" }] } } }] }, {
            version: version.versionNumber,
            description: version.description,
        });
        console.log(msg);
    });
});
