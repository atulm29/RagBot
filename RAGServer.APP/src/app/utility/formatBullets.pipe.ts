import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatBullets'
})
export class FormatBulletsPipe implements PipeTransform {

  transform(value: string): string {
    if (!value) return '';

    const lines = value
      .split('\n')
      .map(line => line.trim())
      .filter(line => line.length > 0);

    const listItems = lines.map(line => `<li>${line}</li>`).join('');

    return `<ul class="list-disc pl-5 space-y-1">${listItems}</ul>`;
  }
}
