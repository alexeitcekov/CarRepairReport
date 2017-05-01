﻿namespace CarRepairReport.Areas.Forum.Managers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using AutoMapper;
    using CarRepairReport.Globals;
    using CarRepairReport.Managers.Interfaces;
    using CarRepairReport.Models.BindingModels.ForumBms;
    using CarRepairReport.Models.Enums;
    using CarRepairReport.Models.Models.CommonModels;
    using CarRepairReport.Models.Models.ForumModels;
    using CarRepairReport.Models.ViewModels.ForumVm;
    using CarRepairReport.Services.Interfaces;

    public class ForumManager : IForumManager
    {
        private IForumService forumService;
        private ICacheManager cacheManager;
        private IUserService userService;

        public ForumManager(IForumService forumService, ICacheManager cacheManager, IUserService userService)
        {
            this.forumService = forumService;
            this.cacheManager = cacheManager;
            this.userService = userService;
        }

        public void SetFilter(HttpSessionStateBase session, ForumFilterBm bm)
        {
            session[CRRConfig.CurrentForumPage] = 0;

            if (string.IsNullOrWhiteSpace(bm.Title) && string.IsNullOrWhiteSpace(bm.Content) && 
                string.IsNullOrWhiteSpace(bm.Tags) && string.IsNullOrWhiteSpace(bm.Category))
            {
                session[CRRConfig.ForumFilter] = null;
            }

            session[CRRConfig.ForumFilter] = bm;
            
        }

        public PostWrapperVm GetPosts(HttpSessionStateBase session)
        {
            IQueryable<Post> entityPosts = null;

            var vm = new PostWrapperVm();
            
            var filter = session[CRRConfig.ForumFilter] as ForumFilterBm;
            var currentPage = 0;

            if (int.TryParse(session[CRRConfig.CurrentForumPage] as string, out currentPage))
            {
                //currentPage++;
            }

            if (filter == null || 
                (string.IsNullOrWhiteSpace(filter.Title) && string.IsNullOrWhiteSpace(filter.Content) &&
                string.IsNullOrWhiteSpace(filter.Tags) && string.IsNullOrWhiteSpace(filter.Category)))
            {
                var entityPostsConnectionIsClose = this.forumService.GetPosts()
                    .Where(x => x.IsQuestion && !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedOn)
                    .Skip(currentPage)
                    .Take(CRRConfig.NumberOfForumPostsPerPage).ToArray();

                var mvss = Mapper.Map<IEnumerable<Post>, IEnumerable<PostVm>>(entityPostsConnectionIsClose);
                
                vm.Posts = mvss;
                vm.Page = currentPage;

                //session[CRRConfig.CurrentForumPage]

                return vm;
            }

            //entityPosts = this.forumService.GetPosts();
            //var vms = Mapper.Map<IEnumerable<Post>, IEnumerable<PostVm>>(entityPosts);
            //var tempCollection = new HashSet<PostVm>();

            var tempCollection = new HashSet<Post>();

            var vms = this.forumService.GetPosts().Where(x => !x.IsDeleted);
            
            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                foreach (var postVm in vms)
                {
                    if (postVm.Title.Contains(filter.Title))
                    {
                        tempCollection.Add(postVm);
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(filter.Content))
            {
                foreach (var postVm in vms)
                {
                    if (postVm.Content.Contains(filter.Content))
                    {
                        tempCollection.Add(postVm);
                    }
                }

            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                var categories = filter.Category.Split(' ');

                foreach (var c in categories)
                {
                    foreach (var postVm in vms)
                    {
                        if (postVm.Categories.Any(x => x.Name == c))
                        {
                            tempCollection.Add(postVm);
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.Tags))
            {
                var tags = filter.Tags.Split(' ').Select(x => x.ToLower());

                foreach (var t in tags)
                {
                    foreach (var postVm in vms)
                    {
                        if (postVm.Tags.Any(x => x.Name == t))
                        {
                            tempCollection.Add(postVm);
                        }
                    }
                }
            }

            var  takeOnlyOriginals = new HashSet<Post>();

            foreach (var tempPost in tempCollection)
            {
                if (tempPost.IsQuestion)
                {
                    takeOnlyOriginals.Add(tempPost);
                }
                else
                {
                    takeOnlyOriginals.Add(tempPost.Original);
                }
                
            }

            if (takeOnlyOriginals.Count == 0)
            {
                vm.Posts = new List<PostVm>();
                return vm;
            }

            var filtered = Mapper.Map<IEnumerable<Post>, IEnumerable<PostVm>>(takeOnlyOriginals);

            var result = filtered.OrderByDescending(x => x.CreatedOn).Skip(currentPage).Take(CRRConfig.NumberOfForumPostsPerPage).ToList();
            
            if (!result.Any())
            {
                vm.Page = 0;
                session[CRRConfig.CurrentForumPage] = 0;
            }
            else
            {
                vm.Page = currentPage;
            }

            vm.Posts = result;

            return vm;
        }

        public ICollection<string> GetCategories(string language)
        {
            var categories = this.forumService.GetCategories().Where(x => !x.IsDeleted).Select(s => s.Name);

            var resources = this.cacheManager.GetLanguageResources(language);
            
            var translatedCategories = new List<string>();

            translatedCategories.Add(string.Empty);

            foreach (var category in categories)
            {
                var resourceValue = resources.FirstOrDefault(x => x.Key == category && x.LanguageCode == language);
                var langValue = resourceValue != null ? resourceValue.Value : category;
                
                translatedCategories.Add(langValue);
            }

            translatedCategories.Sort();

            return translatedCategories;
        }

        public bool CreatePost(CreatePostBm bm, string appUserId, string languageCode)
        {
            if (string.IsNullOrEmpty(appUserId))
            {
                return false;
            }

            var resources = this.cacheManager.GetLanguageResources();

            if (resources == null)
            {
                return false;
            }

            var categoryDto = resources.FirstOrDefault(x => x.Value == bm.Category);

            if (categoryDto == null)
            {
                return false;
            }

            var entityCategory = this.forumService.GetCategoryBySystemName(categoryDto.Key);

            if (entityCategory == null && entityCategory.Type != BelongTo.Forum)
            {
                return false;
            }

            var user = this.userService.GetUserByAppId(appUserId);

            if (user == null)
            {
                return false;
            }

            var post = new Post()
            {
                AuthorId = user.Id,
                Author = user,
                Title = bm.Title,
                Content = bm.Content,
                IsQuestion = true
            };

            user.Posts.Add(post);
            post.Categories.Add(entityCategory);
            entityCategory.Posts.Add(post);
            
            // set tags

            if (!string.IsNullOrWhiteSpace(bm.Tags))
            {
                var userTags = Regex.Split(bm.Tags.Trim(), "\\s+");

                var entityTags = this.forumService.GetTags();

                foreach (var userTag in userTags)
                {
                    var tag = entityTags.FirstOrDefault(x => x.Name == userTag.ToLower());

                    if (tag == null)
                    {
                        tag = new Tag()
                        {
                            Name = userTag.ToLower(),
                            Type = BelongTo.Forum
                        };
                    }

                    post.Tags.Add(tag);
                    tag.Posts.Add(post);
                }
            }
            
            bool isAdded = this.forumService.AddPost(post);

            if (!isAdded)
            {
                return false;
            }

            return true;
        }

        public ViewPostVm GetPost(string title)
        {
            var post = this.forumService.GetPostByTitle(title);

            if (post == null)
            {
                return null;
            }
            
            var vm = Mapper.Map<Post, ViewPostVm>(post);

            foreach (var child in vm.Children)
            {
                child.CssStyleColor = "bg-custom-forum";
            }
            
            return vm;

        }

        public string CreateAnswer(PostAnswerBm bm, string appUserId)
        {
            var user = this.userService.GetUserByAppId(appUserId);

            if (user == null)
            {
                return null;
            }

            Post postEntity = this.forumService.GetPostById(bm.Id);

            if (postEntity == null)
            {
                return null;
            }

            Post originalPost = null;

            if (postEntity.IsQuestion)
            {
                originalPost = postEntity;
            }
            else
            {
                originalPost = postEntity.Original;
            }

            var answer = new Post()
            {
                Author = user,
                AuthorId = user.Id,
                Categories = originalPost.Categories,
                Tags = originalPost.Tags,
                Content = bm.Content,
                Parent = postEntity,
                ParentId = postEntity.Id,
                OriginalId = originalPost.Id,
                Original = originalPost
            };

            postEntity.Children.Add(answer);

            bool isAdded = this.forumService.AddPost(answer);

            if (!isAdded)
            {
                return null;
            }

            var webLink = originalPost.WebLink();

            return webLink;
        }

        public ViewPostVm GetPostById(int id)
        {
            var post = this.forumService.GetPostById(id);

            if (post == null)
            {
                return null;
            }

            var vm = Mapper.Map<Post, ViewPostVm>(post);

            string cssColor = this.cacheManager.GetCssColor();

            foreach (var child in vm.Children)
            {
                child.CssStyleColor = cssColor;
            }

            return vm;
        }
    }
}