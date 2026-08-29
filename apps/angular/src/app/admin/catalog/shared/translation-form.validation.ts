import { AbstractControl } from '@angular/forms';

export function hasDuplicateTranslationLanguages(controls: AbstractControl[]): boolean {
  const languages = controls
    .map((control) => normalizeLanguage(control.get('language')?.value))
    .filter((language) => language.length > 0);

  return new Set(languages).size !== languages.length;
}

export function hasMissingDefaultLanguageTranslation(
  controls: AbstractControl[],
  defaultLanguage: string | null
): boolean {
  const normalizedDefaultLanguage = normalizeLanguage(defaultLanguage);
  if (!normalizedDefaultLanguage) {
    return false;
  }

  return !controls.some((control) => {
    const language = normalizeLanguage(control.get('language')?.value);
    const name = String(control.get('name')?.value ?? '').trim();

    return language === normalizedDefaultLanguage && name.length > 0;
  });
}

/** Same as {@link hasMissingDefaultLanguageTranslation} but reads a `displayName` control (option labels). */
export function hasMissingDefaultLanguageDisplayName(
  controls: AbstractControl[],
  defaultLanguage: string | null
): boolean {
  const normalizedDefaultLanguage = normalizeLanguage(defaultLanguage);
  if (!normalizedDefaultLanguage) {
    return false;
  }

  return !controls.some((control) => {
    const language = normalizeLanguage(control.get('language')?.value);
    const displayName = String(control.get('displayName')?.value ?? '').trim();

    return language === normalizedDefaultLanguage && displayName.length > 0;
  });
}

function normalizeLanguage(language: unknown): string {
  return String(language ?? '').trim().toLowerCase();
}
