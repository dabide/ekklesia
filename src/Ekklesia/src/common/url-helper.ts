import { YouTubeService } from './youtube-service';
import { LogManager, autoinject } from 'aurelia-framework';

const logger = LogManager.getLogger('url-helper');

@autoinject()
export class UrlHelper {
  youTubeRegExp: RegExp = new RegExp('(www\.)?youtube.com', 'i');
  facebookRegExp: RegExp = new RegExp('(www\.)?facebook.com', 'i');
 
  constructor(private youTubeService: YouTubeService) {
  }

  getIcon(url: string): string {
    if (this.isYouTube(url)) return 'fab fa-youtube';
    if (this.isFacebook(url)) return 'fab fa-facebook';
    return 'fas fa-globe';
  }

  isYouTube(url) {
    return this.youTubeRegExp.test(new URL(url).hostname);
  }

  isFacebook(url) {
    return this.facebookRegExp.test(new URL(url).hostname);
  }

  enhance(item: any) {
    if (this.isYouTube(item.url)) {

      return this.youTubeService.getInfo(item.url)
        .then(data => {
          let newUrl = new URL(item.url);
          let video = newUrl.searchParams.get('v');
          if (video == null) return item;
          newUrl.searchParams.delete('v');
          newUrl.pathname = `/embed/${video}`;
          newUrl.searchParams.append('autoplay', '1');
          item.url = item.url;// newUrl.toString();
          item.icon = this.getIcon(item.url);
          item.title = data.title;
          item.author = data.author_name;
          item.thumbnail = data.thumbnail_url;

          logger.debug('item', item);
          return item;
        });
    }

    return Promise.resolve(item);
  }
} 
